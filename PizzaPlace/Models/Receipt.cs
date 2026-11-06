namespace PizzaPlace.Models;
public record Receipt(PizzaOrder Order, double Price, int CookingTime, Guid Id);
