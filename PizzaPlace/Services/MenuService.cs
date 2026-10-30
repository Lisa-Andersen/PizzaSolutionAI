using PizzaPlace.Models.Types;

namespace PizzaPlace.Services;

public class MenuService : IMenuService
{
    public Menu GetMenu(DateTimeOffset menuDate)
    {
        var menuItems = menuDate.TimeOfDay >= TimeSpan.FromHours(11) &&
                        menuDate.TimeOfDay < TimeSpan.FromHours(14)
            ? LuchMenu()
            : StandardMenu();
        return new Menu("Title", menuItems);
    }
    protected static ComparableList<MenuItem> StandardMenu()
    {
        var MenuItems = new ComparableList<MenuItem>();
        MenuItems.Add(new MenuItem("Prosciutto", PizzaRecipeType.StandardPizza, 14));
        MenuItems.Add(new MenuItem("Sausage", PizzaRecipeType.StandardPizza, 13));
        MenuItems.Add(new MenuItem("White Mushroom", PizzaRecipeType.StandardPizza, 13));
        MenuItems.Add(new MenuItem("Spinach and Feta", PizzaRecipeType.StandardPizza, 14));
        MenuItems.Add(new MenuItem("Buffalo Ranch", PizzaRecipeType.StandardPizza, 15));
        MenuItems.Add(new MenuItem("Chicken Alfredo", PizzaRecipeType.StandardPizza, 15));
        MenuItems.Add(new MenuItem("Artichoke", PizzaRecipeType.StandardPizza, 13));
        MenuItems.Add(new MenuItem("Bacon and Onion", PizzaRecipeType.StandardPizza, 14));
        MenuItems.Add(new MenuItem("Mediterranean", PizzaRecipeType.StandardPizza, 15));
        MenuItems.Add(new MenuItem("Spicy Italian", PizzaRecipeType.StandardPizza, 14));
        MenuItems.Add(new MenuItem("Roasted Garlic", PizzaRecipeType.StandardPizza, 12));
        MenuItems.Add(new MenuItem("Garden Special", PizzaRecipeType.StandardPizza, 13));

        return MenuItems;
    }
    protected static ComparableList<MenuItem> LuchMenu() {
        var MenuItems = new ComparableList<MenuItem>();
        MenuItems.Add(new MenuItem("Margherita", PizzaRecipeType.StandardPizza, 10));
        MenuItems.Add(new MenuItem("Pepperoni", PizzaRecipeType.StandardPizza, 12));
        MenuItems.Add(new MenuItem("Four Cheese", PizzaRecipeType.StandardPizza, 13));
        MenuItems.Add(new MenuItem("Vegetarian", PizzaRecipeType.StandardPizza, 11));
        MenuItems.Add(new MenuItem("Hawaiian", PizzaRecipeType.StandardPizza, 12));
        MenuItems.Add(new MenuItem("BBQ Chicken", PizzaRecipeType.StandardPizza, 14));
        MenuItems.Add(new MenuItem("Meat Lovers", PizzaRecipeType.StandardPizza, 15));
        MenuItems.Add(new MenuItem("Buffalo Chicken", PizzaRecipeType.StandardPizza, 14));
        MenuItems.Add(new MenuItem("Mushroom", PizzaRecipeType.StandardPizza, 11));
        MenuItems.Add(new MenuItem("Pesto", PizzaRecipeType.StandardPizza, 13));
        MenuItems.Add(new MenuItem("Supreme", PizzaRecipeType.StandardPizza, 15));
        MenuItems.Add(new MenuItem("白 Pizza", PizzaRecipeType.StandardPizza, 12));
        
        return MenuItems;
    }
}
 