using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Controllers;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;

namespace PizzaPlace.Test.Controllers;

[TestClass]
public class RestockingControllerTest
{
    private static RestockingController GetRepository(Mock<IStockRepository> stockRepository) =>
        new(stockRepository.Object);

    [TestMethod]
    public async Task Restock_AddsItemsToStock()
    {
        // Arrange
        var anchovie = new StockDto(StockType.Anchovies, 5);
        var bacon = new StockDto(StockType.Bacon, 5);
        var chockolate = new StockDto(StockType.Chocolate, 5);
        var stock = new ComparableList<StockDto>([anchovie, chockolate]);

        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);
        stockRepository.Setup(s => s.AddToStock(anchovie)).ReturnsAsync(anchovie);
        stockRepository.Setup(s => s.AddToStock(chockolate)).ReturnsAsync(chockolate);
        stockRepository.Setup(s => s.AddToStock(bacon)).ReturnsAsync(bacon);

        var controller = GetRepository(stockRepository);

        // Act
        var actual = await controller.Restock(stock);

        // Assert
        Assert.IsInstanceOfType<OkResult>(actual);
    }
}