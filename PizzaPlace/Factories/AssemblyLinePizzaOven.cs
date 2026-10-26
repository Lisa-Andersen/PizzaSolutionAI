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
}
