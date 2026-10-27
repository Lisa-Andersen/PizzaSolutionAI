using PizzaPlace.Models;
using PizzaPlace.Pizzas;

namespace PizzaPlace.Factories;

/// <summary>
/// Produces pizza on one giant revolving surface. Downside is, that all pizzas must have the same cooking time, when prepared at the same time.
/// </summary>
public class GiantRevolvingPizzaOven(TimeProvider timeProvider) : PizzaOven(timeProvider)
{
    private const int GiantRevolvingPizzaOvenCapacity = 120;

    protected override int Capacity => GiantRevolvingPizzaOvenCapacity;
    private List<HashSet<Guid>> _pizzasWhichCanBeCookedTogether = new();
    private HashSet<Guid> _currentGroupOfPizzas = new();

    protected override void PlanPizzaMaking(IEnumerable<(PizzaRecipeDto Recipe, Guid Guid)> recipeOrders)
    {
        var ovenGroups = recipeOrders.GroupBy(x => x.Recipe.CookingTimeMinutes);
        foreach (var ovenGroup in ovenGroups)
        {
            HashSet<Guid> group = ovenGroup.Select(p => p.Guid).ToHashSet();
            _pizzasWhichCanBeCookedTogether.Add(group);

            foreach (var pizza in ovenGroup)
            {
                var (dto, guid) = pizza;
                _pizzaQueue.Enqueue((MakePizza(dto), guid));
            }
        }
    }

    private Func<Task<Pizza?>> MakePizza(PizzaRecipeDto recipe) => async () =>
    {
        await CookPizza(recipe.CookingTimeMinutes);
        return GetPizza(recipe.RecipeType);
    };


    protected override bool CanBeCookedNow(Guid guid)
    {
        if (_pizzasInOven.Count == 0)
        {
            _currentGroupOfPizzas = _pizzasWhichCanBeCookedTogether.Find(x => x.Contains(guid));
            if (_currentGroupOfPizzas == null)
            {
                throw new PizzaException($"Pizza with guid {guid} is not part of any group of pizzas which can be cooked together.");
            }
            
            return true;
        }
        return _currentGroupOfPizzas.Contains(guid);
    }
}
