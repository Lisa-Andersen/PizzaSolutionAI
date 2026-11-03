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
    private Queue<int> _batchSizeQueue = new();
    private int _currentBatchSize = 0;

    protected override void PlanPizzaMaking(IEnumerable<(PizzaRecipeDto Recipe, Guid Guid)> recipeOrders)
    {
        var ovenGroups = recipeOrders.GroupBy(x => x.Recipe.CookingTimeMinutes);
        foreach (var ovenGroup in ovenGroups)
        {
            int batchSize = ovenGroup.Count();
            if (batchSize > GiantRevolvingPizzaOvenCapacity)
            {
                _batchSizeQueue.Enqueue(GiantRevolvingPizzaOvenCapacity);
                batchSize -= GiantRevolvingPizzaOvenCapacity;
            }
            _batchSizeQueue.Enqueue(batchSize);

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

    /**
     * Determines if a pizza can be cooked now.
     * The planning has made a queue of batch sizes.
     * If we are within the number of pizzas in batch we can add it to the oven.
     */
    protected override bool CanBeCookedNow()
    {
        if (_pizzasInOven.Count ==  0 && _batchSizeQueue.TryDequeue(out _currentBatchSize)) {
            return true;
        }
        return _pizzasInOven.Count < _currentBatchSize;
    }

    public override int TotalCookingTime(ComparableList<PizzaPrepareOrder> order)
    {
        var totalCookingTime = 0;
        List<PizzaRecipeDto> pizzas = order
            .SelectMany(o => Enumerable.Range(0, o.OrderAmount).Select(_ => (o.RecipeDto)))
            .ToList();
        var ovenGroups = pizzas.GroupBy(x => x.CookingTimeMinutes);
        foreach (var ovenGroup in ovenGroups)
        {
            totalCookingTime += (int) Math.Ceiling(((double) ovenGroup.Count()) / GiantRevolvingPizzaOvenCapacity)*ovenGroup.Key;
        }
        return totalCookingTime;
    }
}
