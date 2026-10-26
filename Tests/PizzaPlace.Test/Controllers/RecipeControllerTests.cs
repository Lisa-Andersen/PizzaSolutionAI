using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Controllers;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Services;

namespace PizzaPlace.Test.Controllers;

[TestClass]
public class RecipeControllerTests
{
    private static RecipeController GetController(Mock<IRecipeService> recipeService) =>
    new(recipeService.Object);

    [TestMethod]
    public async Task AddRecipe_ReturnsLongId()
    {
        // Arrange
        PizzaRecipeDto recipe = new PizzaRecipeDto(PizzaRecipeType.EmptyPizza, new ComparableList<StockDto>(), 1);

        var recipeService = new Mock<IRecipeService>(MockBehavior.Strict);
        recipeService.Setup(x => x.AddRecipe(recipe))
            .ReturnsAsync(recipe.Id);

        var controller = GetController(recipeService);

        // Act
        var result = await controller.AddRecipe(recipe);

        // Assert
        Assert.IsInstanceOfType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.IsInstanceOfType<long>(okResult.Value);
        Assert.AreEqual<long>(recipe.Id, (long) okResult.Value);
        recipeService.VerifyAll();
    }
}

