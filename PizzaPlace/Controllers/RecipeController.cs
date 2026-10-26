using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Models;
using PizzaPlace.Services;


namespace PizzaPlace.Controllers
{
    [Route("api/recipe")]
    [ApiController]
    public class RecipeController(IRecipeService recipeService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> AddRecipe(PizzaRecipeDto recipe)
        {
            var recipeId = await recipeService.AddRecipe(recipe);
            return Ok(recipeId);
        }
    }
}
