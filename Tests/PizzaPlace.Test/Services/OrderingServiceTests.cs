using PizzaPlace.Factories;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Pizzas;
using PizzaPlace.Services;

namespace PizzaPlace.Test.Services;

[TestClass]
public class OrderingServiceTests
{
    private static DateTimeOffset dateTimeOffset = new DateTimeOffset();
    private static OrderingService GetService(IMenuService menuService, IStockService stockService, IRecipeService recipeService, IPizzaOven pizzaOven) =>
        new OrderingService(menuService, stockService, recipeService, pizzaOven);
    private StringWriter _stringWriter;

    [TestInitialize]
    public void Init()
    {
        _stringWriter = new StringWriter();
        Console.SetOut(_stringWriter);
    }

    [TestMethod]
    public async Task HandlePizzaOrder()
    {
        // Arrange

        // Arrange Order
        ComparableList<PizzaAmount> requests =
        [
            new PizzaAmount(PizzaRecipeType.StandardPizza, 54),
            new PizzaAmount(PizzaRecipeType.ExtremelyTastyPizza, 2),
            new PizzaAmount(PizzaRecipeType.StandardPizza, 0),
            new PizzaAmount(PizzaRecipeType.StandardPizza, 4),
        ];
        var order = new PizzaOrder(requests);

        // Arrange Recipe
        var standardRecipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza,
            [
                new StockDto(StockType.Dough, 2),
                new StockDto(StockType.Tomatoes, 1),
            ], 10);
        var tastyRecipe = new PizzaRecipeDto(PizzaRecipeType.ExtremelyTastyPizza,
            [
                new StockDto(StockType.UnicornDust, 1),
                new StockDto(StockType.BellPeppers, 2),
            ], 15);
        ComparableList<PizzaRecipeDto> recipes = [standardRecipe, tastyRecipe];

        // Arrange Stock
        ComparableList<StockDto> returnedStock = [new StockDto(StockType.Anchovies, 2)]; // Doesn't matter that it doesn't match recipes.

        // Arrange the mocked returned orders
        ComparableList<PizzaPrepareOrder> prepareOrders =
        [
            new PizzaPrepareOrder(standardRecipe, 58),
            new PizzaPrepareOrder(tastyRecipe, 2),
        ];
        ComparableList<MenuItem> menuItems = [
                new MenuItem("", PizzaRecipeType.StandardPizza, 2),
                new MenuItem("", PizzaRecipeType.ExtremelyTastyPizza, 3)
            ];
        var menu = new Menu("Title", menuItems);

        var pizzas = new List<Pizza>{};

        var menuService = new Mock<IMenuService>(MockBehavior.Strict);
        var stockService = new Mock<IStockService>(MockBehavior.Strict);
        var recipeService = new Mock<IRecipeService>(MockBehavior.Strict);
        var pizzaOven = new Mock<IPizzaOven>(MockBehavior.Strict);

        menuService.Setup(x => x.GetMenu(dateTimeOffset))
            .Returns(menu);
        recipeService.Setup(x => x.GetPizzaRecipes(order))
            .ReturnsAsync(recipes);
        stockService.Setup(x => x.HasInsufficientStock(order, recipes))
            .ReturnsAsync(false);
        stockService.Setup(x => x.GetStock(order, recipes))
            .ReturnsAsync(returnedStock);
       pizzaOven.Setup(x => x.PreparePizzas(prepareOrders, returnedStock))
            .ReturnsAsync(pizzas);
        pizzaOven.Setup(x => x.TotalCookingTime(prepareOrders))
            .Returns(10);

        var service = GetService(menuService.Object, stockService.Object, recipeService.Object, pizzaOven.Object);

        var expectedPrice = 2 * 58 + 3 * 2; // StandardPizza price * amount + ExtremelyTastyPizza price * amount
        var expectedReceipt = new Receipt(order, expectedPrice, 25, new Guid());

        // Act
        Receipt actual = await service.HandlePizzaOrder(dateTimeOffset, order);

        // Assert
        Assert.AreEqual(order, actual.Order);
        Assert.AreEqual(expectedPrice, actual.Price);
        Assert.AreEqual(10, actual.CookingTime);
        var receiptId = actual.Id;
        Assert.IsTrue(_stringWriter.ToString().Contains($"ORDER {actual.Id} IS READY!"));

        menuService.VerifyAll();
        stockService.VerifyAll();
        recipeService.VerifyAll();
        pizzaOven.VerifyAll();
    }

    [TestMethod]
    public async Task HandlePizzaOrder_InsufficientStock()
    {
        // Arrange
        ComparableList<PizzaAmount> requests =
        [
            new PizzaAmount(PizzaRecipeType.ExtremelyTastyPizza, 2),
        ];

        var order = new PizzaOrder(requests);
        var tastyRecipe = new PizzaRecipeDto(PizzaRecipeType.ExtremelyTastyPizza,
            [
                new StockDto(StockType.UnicornDust, 1),
                new StockDto(StockType.BellPeppers, 2),
            ], 15);

        ComparableList<PizzaRecipeDto> recipes = [tastyRecipe];

        ComparableList<MenuItem> menuItems = [
        new MenuItem("", PizzaRecipeType.StandardPizza, 2),
                new MenuItem("", PizzaRecipeType.ExtremelyTastyPizza, 3)
    ];
        var menu = new Menu("Title", menuItems);


        var menuService = new Mock<IMenuService>(MockBehavior.Strict);
        var stockService = new Mock<IStockService>(MockBehavior.Strict);
        var recipeService = new Mock<IRecipeService>(MockBehavior.Strict);
        var pizzaOven = new Mock<IPizzaOven>(MockBehavior.Strict);

        recipeService.Setup(x => x.GetPizzaRecipes(order))
            .ReturnsAsync(recipes);
        stockService.Setup(x => x.HasInsufficientStock(order, recipes))
            .ReturnsAsync(true);

        var service = GetService(menuService.Object, stockService.Object, recipeService.Object, pizzaOven.Object);

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => service.HandlePizzaOrder(dateTimeOffset, order));

        // Assert
        Assert.AreEqual("Unable to take in order. Insufficient stock.", ex.Message);
        stockService.VerifyAll();
        recipeService.VerifyAll();
        pizzaOven.VerifyAll();
    }


    [TestMethod]
    public async Task HandlePizzaOrder_BrokenRecipeServiceDoesNotReturnRecipe()
    {
        // Arrange
        ComparableList<PizzaAmount> requests =
        [
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1),
        ];
        var order = new PizzaOrder(requests);
        ComparableList<PizzaRecipeDto> recipes = [];
        ComparableList<StockDto> returnedStock = [new StockDto(StockType.Anchovies, 2)]; // Doesn't matter that it doesn't match recipes.

        var menuService = new Mock<IMenuService>(MockBehavior.Strict);
        var stockService = new Mock<IStockService>(MockBehavior.Strict);
        var recipeService = new Mock<IRecipeService>(MockBehavior.Strict);
        var pizzaOven = new Mock<IPizzaOven>(MockBehavior.Strict);

        recipeService.Setup(x => x.GetPizzaRecipes(order))
            .ReturnsAsync(recipes);
        stockService.Setup(x => x.HasInsufficientStock(order, recipes))
            .ReturnsAsync(false);
        stockService.Setup(x => x.GetStock(order, recipes))
            .ReturnsAsync(returnedStock);

        var service = GetService(menuService.Object, stockService.Object, recipeService.Object, pizzaOven.Object);

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => service.HandlePizzaOrder(dateTimeOffset, order));

        // Assert
        Assert.IsTrue(ex.Message.Contains("Recipe service did not return a recipe for StandardPizza which was expected."));
        stockService.VerifyAll();
        recipeService.VerifyAll();
        pizzaOven.VerifyAll();
    }
}
