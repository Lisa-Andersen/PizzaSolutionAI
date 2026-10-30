using PizzaPlace.Services;

namespace PizzaPlace.Test.Services;

[TestClass]
public class MenuServiceTest : MenuService
{
    [TestMethod]
    public void GetMenuAtLunch_ReturnsLunchMenu()
    {
        // Arrange
        DateTime dateTime = new DateTime(2024, 6, 1, 12, 0, 0);
        DateTimeOffset timeOfDay = new DateTimeOffset(dateTime);

        var StandardItems = new ComparableList<MenuItem>();
        var LunchItems = LuchMenu();

        var menuService = new MenuService();

        // Assert
        Assert.AreEqual(LunchItems, menuService.GetMenu(timeOfDay).Items);
    }

    [TestMethod]
    public void GetMenuAtLunch_ReturnsStandardMenu()
    {
        // Arrange
        DateTime dateTime = new DateTime(2024, 6, 1, 14, 0, 0);
        DateTimeOffset timeOfDay = new DateTimeOffset(dateTime);

        var StandardItems = new ComparableList<MenuItem>();
        var LunchItems = StandardMenu();

        var menuService = new MenuService();

        // Assert
        Assert.AreEqual(LunchItems, menuService.GetMenu(timeOfDay).Items);
    }
}