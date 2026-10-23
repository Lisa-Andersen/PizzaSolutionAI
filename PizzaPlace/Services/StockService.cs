using PizzaPlace.Extensions;
using PizzaPlace.Models;
using PizzaPlace.Repositories;

namespace PizzaPlace.Services;

public class StockService(IStockRepository stockRepository) : IStockService
{
    public Task<bool> HasInsufficientStock(PizzaOrder order, ComparableList<PizzaRecipeDto> recipeDtos)
    {
        ComparableList<StockDto> necessaryIngredients = IngredientsFromOrder(order, recipeDtos);

        // Check that you have enough stock for each ingredient
        foreach (var ingredient in necessaryIngredients)
        {
            var stock = stockRepository.GetStock(ingredient.StockType);
            if (stock.Result.Amount < ingredient.Amount)
            {
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }

    public async Task<ComparableList<StockDto>> GetStock(PizzaOrder order, ComparableList<PizzaRecipeDto> recipeDtos)
    {
        if (await HasInsufficientStock(order, recipeDtos))
            throw new Exception("Insufficient stock for the order.");

        ComparableList<StockDto> necessaryIngredients = IngredientsFromOrder(order, recipeDtos);
        var takeStockTasks = necessaryIngredients
            .Select(ing => stockRepository.TakeStock(ing.StockType, ing.Amount));
        await Task.WhenAll(takeStockTasks);

        return necessaryIngredients;
    }

    private ComparableList<StockDto> IngredientsFromOrder(PizzaOrder order, ComparableList<PizzaRecipeDto> recipeDtos)
    {
        ComparableList<PizzaAmount> requestedPizzas = order.RequestedOrder;
        ComparableList<StockDto> necessaryIngredients = new ComparableList<StockDto>();
        
        foreach (var pizzas in requestedPizzas)
        {
            ComparableList<StockDto> ingredients = recipeDtos
                .First(recipe => recipe.RecipeType == pizzas.PizzaType)
                .Ingredients;
            var ingredients2 = ingredients.Select(i => new StockDto(i.StockType,i.Amount * pizzas.Amount));
            necessaryIngredients.AddRange(ingredients2);
        }
        return PizzaHelperExtensions.GatherSameTypeOfStock(necessaryIngredients).ToComparableList();
    }

}
