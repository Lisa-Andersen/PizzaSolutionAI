using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Controllers;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Services;

namespace PizzaPlace.Test.Controllers;

[TestClass]
public class OrderingControllerTests
{
    private static OrderingController GetController(Mock<TimeProvider> timeProvider, Mock<IOrderingService> orderingService) =>
        new(timeProvider.Object, orderingService.Object);

    [TestMethod]
    public async Task PlacePizzaOrder()
    {
        // Arrange
        var order = new PizzaOrder([new PizzaAmount(PizzaRecipeType.EmptyPizza, 1)]);
        Receipt someReceipt = new Receipt(order, 1, 1, new Guid());
        DateTimeOffset menuDate = new DateTimeOffset();

        var timeProvider = new Mock<TimeProvider>(MockBehavior.Strict);
        timeProvider.Setup(t => t.GetUtcNow()).Returns(menuDate);

        var orderingService = new Mock<IOrderingService>(MockBehavior.Strict);
        orderingService.Setup(x => x.HandlePizzaOrder(menuDate, order))
            .ReturnsAsync(someReceipt); // Doesn't matter.

        var controller = GetController(timeProvider, orderingService);

        // Act
        var actual = await controller.PlacePizzaOrder(order);

        // Assert
        Assert.IsInstanceOfType<OkObjectResult>(actual);
        orderingService.VerifyAll();
        timeProvider.VerifyAll();
    }
}
