using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Pizzas;

namespace PizzaPlace.Factories;

/// <summary>
/// Producing one line of pizza. 
/// Taking 7 minutes to setup - and then 5 minutes less for every subsequent pizza of the same recipe type to a minimum of 4 minutes.
/// </summary>
public class AssemblyLinePizzaOven(TimeProvider timeProvider) : PizzaOven(timeProvider)
{
    private const int AssemblyLineCapacity = 1;
    public const int SetupTimeMinutes = 7;
    public const int SubsequentPizzaTimeSavingsInMinutes = 5;
    public const int MinimumCookingTimeMinutes = 4;

    protected override int Capacity => AssemblyLineCapacity;

    protected override void PlanPizzaMaking(IEnumerable<(PizzaRecipeDto Recipe, Guid Guid)> recipeOrders)
    {
        recipeOrders = recipeOrders.OrderBy(r => r.Recipe.RecipeType);
        var prevRecipeType = PizzaRecipeType.EmptyPizza;
        var prevCookingTime = 0;
        foreach (var (recipe, orderGuid) in recipeOrders)
        {
            var cookingTime = recipe.CookingTimeMinutes;
            if (recipe.RecipeType == prevRecipeType)
            {
                cookingTime = Math.Max(prevCookingTime - 5, MinimumCookingTimeMinutes);
                _pizzaQueue.Enqueue((MakePizza(cookingTime, recipe), orderGuid));
                prevCookingTime = cookingTime;
            }
            else
            {
                cookingTime = recipe.CookingTimeMinutes + SetupTimeMinutes;
                _pizzaQueue.Enqueue((MakePizza(cookingTime, recipe), orderGuid));
                prevCookingTime = cookingTime;
            }
            prevRecipeType = recipe.RecipeType;
        }
    }

    private Func<Task<Pizza?>> MakePizza(int cookingTime, PizzaRecipeDto recipe) => async () =>
    {
        await CookPizza(cookingTime);
        
        return GetPizza(recipe.RecipeType);
    };

    public override int TotalCookingTime(ComparableList<PizzaPrepareOrder> order)
    {
        var totalCookingTime = 0;
        List<PizzaRecipeDto> pizzas = order
            .SelectMany(o => Enumerable.Range(0, o.OrderAmount).Select(_ => (o.RecipeDto)))
            .OrderBy(r => r.RecipeType)
            .ToList();

        var prevRecipeType = PizzaRecipeType.EmptyPizza;
        var prevCookingTime = 0;
        foreach (PizzaRecipeDto pizza in pizzas) {
            var cookingTime = pizza.CookingTimeMinutes;
            if (pizza.RecipeType == prevRecipeType)
            {
                cookingTime = Math.Max(prevCookingTime - 5, MinimumCookingTimeMinutes);
                prevCookingTime = cookingTime;
            }
            else
            {
                cookingTime = pizza.CookingTimeMinutes + SetupTimeMinutes;
                prevCookingTime = cookingTime;
            }
            prevRecipeType = pizza.RecipeType;

            // Aggregate cooking time
            totalCookingTime += prevCookingTime;
        }
        return totalCookingTime;
    }
}
