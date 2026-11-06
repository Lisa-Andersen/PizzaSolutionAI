using PizzaPlace.Factories;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Pizzas;

namespace PizzaPlace.Services;

public class OrderingService(
    IMenuService menuService,
    IStockService stockService,
    IRecipeService recipeService,
    IPizzaOven pizzaOven) : IOrderingService
{

    public async Task<Receipt> HandlePizzaOrder(DateTimeOffset menuDate, PizzaOrder order)
    {
        var recipes = await recipeService.GetPizzaRecipes(order);
        if (await stockService.HasInsufficientStock(order, recipes))
            throw new PizzaException("Unable to take in order. Insufficient stock.");

        var stock = await stockService.GetStock(order, recipes);

        // Create an identifier for the order, such that we can track the pizzas in the oven and match them to the order.
        var orderId = Guid.NewGuid();
        var prepareOrder = order.RequestedOrder
            .GroupBy(x => x.PizzaType)
            .Select(x => new PizzaPrepareOrder(GetPizzaRecipe(x.Key, recipes, orderId), x.Aggregate(0, (total, request) => total + request.Amount)))
            .ToComparableList();

        var cookPizza = CookPizzas(prepareOrder, stock, orderId);

        // Give receipt
        double price = GetPrice(menuDate, order);
        int cookingTime = pizzaOven.TotalCookingTime(prepareOrder);
        return new Receipt(order, price, cookingTime, orderId);

        PizzaRecipeDto GetPizzaRecipe(PizzaRecipeType pizzaType, ComparableList<PizzaRecipeDto> recipes, Guid orderId) =>
        recipes.FirstOrDefault(x => x.RecipeType == pizzaType) ??
         throw new PizzaException($"Missing recipe for order {orderId}. Recipe service did not return a recipe for {pizzaType} which was expected.");
    }

    private double GetPrice(DateTimeOffset menuDate, PizzaOrder order) {
        Menu menu = menuService.GetMenu(menuDate);
        ComparableList<MenuItem> menuItems = menu.Items;
        Func<PizzaAmount, double> priceOfPizza = 
            (PizzaAmount rOrder) => menuItems.First(p => p.PizzaType == rOrder.PizzaType).Price* rOrder.Amount;

        return order.RequestedOrder.Aggregate(0.0, (prod, nextOrder) => prod + priceOfPizza(nextOrder));
    }

    public async Task<IEnumerable<Pizza>> CookPizzas(ComparableList<PizzaPrepareOrder> prepareOrder, ComparableList<StockDto> stock, Guid orderId) {
        var pizza = await pizzaOven.PreparePizzas(prepareOrder, stock);
        Console.WriteLine($"ORDER {orderId} IS READY!");
        return pizza;
    }
}
