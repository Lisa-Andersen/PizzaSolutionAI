using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;
using PizzaPlace.Services;

namespace PizzaPlace.Test.Services;

[TestClass]
public class StockServiceTest
{

    private static IStockService GetService(Mock<IStockRepository> stockRepository) =>
        new StockService(stockRepository.Object);


    [TestMethod]
    public async Task HasInsufficientStock_ReturnsTrue_WhenStockIsInsufficient()
    {
        // Arrange
        var order = new PizzaOrder([
            new PizzaAmount(PizzaRecipeType.RarePizza, 1),
            new PizzaAmount(PizzaRecipeType.OddPizza, 2),
            new PizzaAmount(PizzaRecipeType.RarePizza, 20),
        ]);
        var rareRecipe = new PizzaRecipeDto(PizzaRecipeType.RarePizza, [new StockDto(StockType.UnicornDust, 1)], 1);
        var oddRecipe = new PizzaRecipeDto(PizzaRecipeType.OddPizza, [new StockDto(StockType.Sulphur, 10)], 100);
        var recipeDtos = new ComparableList<PizzaRecipeDto> { rareRecipe, oddRecipe };

        var stockRepositoryMock = new Mock<IStockRepository>(MockBehavior.Strict);
        stockRepositoryMock.Setup(r => r.GetStock(StockType.UnicornDust)).ReturnsAsync(new StockDto(StockType.UnicornDust, 0));
        stockRepositoryMock.Setup(r => r.GetStock(StockType.Sulphur)).ReturnsAsync(new StockDto(StockType.Sulphur, 0));

        var stockService = GetService(stockRepositoryMock);

        // Act
        var result = await stockService.HasInsufficientStock(order, recipeDtos);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task HasInsufficientStock_ReturnsFalse_WhenStockIsSufficient()
    {
        // Arrange
        var order = new PizzaOrder([
            new PizzaAmount(PizzaRecipeType.RarePizza, 1),
            new PizzaAmount(PizzaRecipeType.OddPizza, 2),
            new PizzaAmount(PizzaRecipeType.RarePizza, 20),
        ]);

        var rareRecipe = new PizzaRecipeDto(PizzaRecipeType.RarePizza, [new StockDto(StockType.UnicornDust, 1)], 1);
        var oddRecipe = new PizzaRecipeDto(PizzaRecipeType.OddPizza, [new StockDto(StockType.Sulphur, 10)], 10);
        var recipeDtos = new ComparableList<PizzaRecipeDto> { rareRecipe, oddRecipe };

        var stockRepositoryMock = new Mock<IStockRepository>(MockBehavior.Strict);
        stockRepositoryMock.Setup(r => r.GetStock(StockType.UnicornDust)).ReturnsAsync(new StockDto(StockType.UnicornDust, 100));
        stockRepositoryMock.Setup(r => r.GetStock(StockType.Sulphur)).ReturnsAsync(new StockDto(StockType.Sulphur, 200));

        var stockService = GetService(stockRepositoryMock);

        // Act
        var result = await stockService.HasInsufficientStock(order, recipeDtos);

        // Assert
        Assert.IsFalse(result);
    }


    [TestMethod]
    public async Task GetStock_ReturnsExpectedStock_WhenStockExists()
    {
        // Arrange
        var order = new PizzaOrder([
            new PizzaAmount(PizzaRecipeType.RarePizza, 10),
            new PizzaAmount(PizzaRecipeType.OddPizza, 2),
        ]);
        var rareRecipe = new PizzaRecipeDto(PizzaRecipeType.RarePizza, [new StockDto(StockType.UnicornDust, 1)], 1);
        var oddRecipe = new PizzaRecipeDto(PizzaRecipeType.OddPizza, [new StockDto(StockType.Sulphur, 10)], 10);
        var recipeDtos = new ComparableList<PizzaRecipeDto> { rareRecipe, oddRecipe };

        var stockRepositoryMock = new Mock<IStockRepository>(MockBehavior.Strict);
        stockRepositoryMock.Setup(r => r.GetStock(StockType.UnicornDust)).ReturnsAsync(new StockDto(StockType.UnicornDust, 100));
        stockRepositoryMock.Setup(r => r.GetStock(StockType.Sulphur)).ReturnsAsync(new StockDto(StockType.Sulphur, 200));
        stockRepositoryMock.Setup(r => r.TakeStock(StockType.UnicornDust, 10)).ReturnsAsync(new StockDto(StockType.UnicornDust, 10));
        stockRepositoryMock.Setup(r => r.TakeStock(StockType.Sulphur, 20)).ReturnsAsync(new StockDto(StockType.Sulphur, 20));

        var stockService = GetService(stockRepositoryMock);

        var expectedStock = new ComparableList<StockDto>
        {
            new StockDto(StockType.UnicornDust, 10),
            new StockDto(StockType.Sulphur, 20)
        };

        // Act
        var result = await stockService.GetStock(order, recipeDtos);

        // Assert
        Assert.AreEqual(expectedStock, result);
    }

    [TestMethod]
    public async Task GetStock_ThrowsException_WhenStockIsInsufficient()
    {
        // Arrange
        var order = new PizzaOrder([
            new PizzaAmount(PizzaRecipeType.RarePizza, 1),
            new PizzaAmount(PizzaRecipeType.OddPizza, 2),
            new PizzaAmount(PizzaRecipeType.RarePizza, 20),
        ]);
        var rareRecipe = new PizzaRecipeDto(PizzaRecipeType.RarePizza, [new StockDto(StockType.UnicornDust, 1)], 1);
        var oddRecipe = new PizzaRecipeDto(PizzaRecipeType.OddPizza, [new StockDto(StockType.Sulphur, 10)], 10);
        var recipeDtos = new ComparableList<PizzaRecipeDto> { rareRecipe, oddRecipe };

        var stockRepositoryMock = new Mock<IStockRepository>(MockBehavior.Strict);
        stockRepositoryMock.Setup(r => r.GetStock(StockType.UnicornDust)).ReturnsAsync(new StockDto(StockType.UnicornDust, 0));
        stockRepositoryMock.Setup(r => r.GetStock(StockType.Sulphur)).ReturnsAsync(new StockDto(StockType.Sulphur, 0));
        stockRepositoryMock.Setup(r => r.TakeStock(StockType.UnicornDust, 21)).ReturnsAsync(new StockDto(StockType.UnicornDust, 21));
        stockRepositoryMock.Setup(r => r.TakeStock(StockType.Sulphur, 20)).ReturnsAsync(new StockDto(StockType.Sulphur, 20));

        var stockService = GetService(stockRepositoryMock);

        // Act
        var result = async () => await stockService.GetStock(order, recipeDtos);

        // Assert
        var excep = Assert.ThrowsExceptionAsync<Exception>(result);
    }
}
