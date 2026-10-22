using Moq.Protected;
using PizzaPlace.Services;

namespace PizzaPlace.Test.Services;

[TestClass]
public class MenuServiceTest
{
    [TestMethod]
    public static void GetMenuAtLunch_ReturnsLunchMenu()
    {
        // Arrange
        DateTime dateTime = new DateTime(2024, 6, 1, 12, 0, 0);
        DateTimeOffset timeOfDay = new DateTimeOffset(dateTime);

        var StandardItems = new ComparableList<MenuItem>();
        var LunchItems = new ComparableList<MenuItem>();

        var menuService = new Mock<MenuService>(MockBehavior.Strict);
        menuService.Protected().Setup<ComparableList<MenuItem>>("StandardMenu").Returns(StandardItems);
        menuService.Protected().Setup<ComparableList<MenuItem>>("LuchMenu").Returns(LunchItems);

        // Assert
        Assert.AreEqual(LunchItems, menuService.Object.GetMenu(timeOfDay).Items);
    }
}