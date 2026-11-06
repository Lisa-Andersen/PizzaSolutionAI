using PizzaPlace.Models;
using PizzaPlace.Pizzas;
using System.Text.RegularExpressions;

namespace PizzaPlace.Factories;

/// <summary>
/// A normal oven producing each pizza one at a time. Each pizza taking the normal cooking time.
/// </summary>
public class NormalPizzaOven(TimeProvider timeProvider) : PizzaOven(timeProvider)
{
    private const int NormalPizzaOvenCapacity = 4;

    protected override int Capacity => NormalPizzaOvenCapacity;

    protected override void PlanPizzaMaking(IEnumerable<(PizzaRecipeDto Recipe, Guid Guid)> recipeOrders)
    {
        foreach (var (recipe, orderGuid) in recipeOrders)
        {
            _pizzaQueue.Enqueue((MakePizza(recipe), orderGuid));
        }
    }

    private Func<Task<Pizza?>> MakePizza(PizzaRecipeDto recipe) => async () =>
    {
        await CookPizza(recipe.CookingTimeMinutes);

        return GetPizza(recipe.RecipeType);
    };

    public override int TotalCookingTime(ComparableList<PizzaPrepareOrder> order) {
        List<PizzaRecipeDto> pizzas = order
            .SelectMany(o => Enumerable.Range(0, o.OrderAmount)
            .Select(_ => (o.RecipeDto)))
            .OrderByDescending(p => p.CookingTimeMinutes).ToList();
        var pizzaGroups = pizzas.Select((x, i) => new { Index = i, Value = x })
            .GroupBy(i => i.Index/4)
            .Select(x => x.Select(v => v.Value).ToList());

        int totalCookingTime = 0;
        foreach (var group in pizzaGroups) {
            totalCookingTime += group.Select(p => p.CookingTimeMinutes).Max();
        }

        return totalCookingTime;
    }
}
