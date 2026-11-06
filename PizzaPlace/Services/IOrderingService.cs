using PizzaPlace.Models;

namespace PizzaPlace.Services;

public interface IOrderingService
{
    Task<Receipt> HandlePizzaOrder(DateTimeOffset menuDate, PizzaOrder order);
}
