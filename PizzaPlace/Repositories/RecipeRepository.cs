using Npgsql;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;

namespace PizzaPlace.Repositories;

public class RecipeRepository : IRecipeRepository
{

    private static readonly string dataTable = "pizza_recipe";
    private static readonly string ingredientsTable = "ingredients";


    /**
     * Add Recipe
     **/
    public Task<long> AddRecipe(PizzaRecipeDto recipe) {

        if (recipe.CookingTimeMinutes <= 0) {
            throw new PizzaException("Cooking time must be larger than 0.");
        }

        string insertRecipe;
        int id = (int)recipe.Id;
        if (id == 0)
        {
            insertRecipe = $"INSERT INTO {dataTable} (RecipeType, CookingTime) " +
                $"VALUES ('{recipe.RecipeType}', {recipe.CookingTimeMinutes}) " +
                $"RETURNING ID;";
        }
        else
        {
            insertRecipe = $"INSERT INTO {dataTable} (Id, RecipeType, CookingTime) " +
                $"VALUES ({recipe.Id}, '{recipe.RecipeType}', {recipe.CookingTimeMinutes}) " +
                $"RETURNING ID;";
        }

        return DatabaseAddRecipe(insertRecipe, recipe.RecipeType, recipe.Id);
    }


    private async Task<long> DatabaseAddRecipe(string insertRecipe, PizzaRecipeType type, long id)
    { 
        try
        {
            object? idRead = await Database.Database.SqlAsyncRequestObject(insertRecipe);
            if (idRead == null)
                throw new PizzaException("The Id is not found");
            return Convert.ToInt64(idRead);
        }
        catch (Exception) {
            throw new PizzaException($"Recipe already added for {type} or with id {id}.");
        }
    }

    /**
     * Get Recipe
     **/

    public Task<PizzaRecipeDto> GetRecipe(PizzaRecipeType recipeType) {
        return DatabaseGetRecipe(recipeType);
    }

    private async Task<PizzaRecipeDto> DatabaseGetRecipe(PizzaRecipeType recipeType) {
        long id = 0;
        PizzaRecipeType type = PizzaRecipeType.EmptyPizza;
        int cookingTime = 0;

        using (NpgsqlConnection connection = new NpgsqlConnection(Database.Database.ConnectionString))
        {
            string request = $"SELECT id, RecipeType, CookingTime FROM {dataTable} WHERE RecipeType='{recipeType}'";
            await connection.OpenAsync();
            using NpgsqlCommand cmd = new NpgsqlCommand(request, connection);
            NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                id = Convert.ToInt64(reader.GetInt32(0));
                PizzaRecipeType.TryParse(reader.GetString(1), out type);
                cookingTime = reader.GetInt32(2);
            }
        }

        ComparableList<StockDto> ingredients = await GetIngredients(id);
        PizzaRecipeDto recipe = new PizzaRecipeDto(type, ingredients, cookingTime, id);
        return recipe;
    }

    private async Task<ComparableList<StockDto>> GetIngredients(long recipeId)
    {
        string request = $"SELECT id, StockType, Amount FROM {ingredientsTable} WHERE PizzaTypeId={recipeId}";

        using (NpgsqlConnection connection = new NpgsqlConnection(Database.Database.ConnectionString))
        {
            try
            {
                await connection.OpenAsync();
                using NpgsqlCommand cmd = new NpgsqlCommand(request, connection);
                NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

                ComparableList<StockDto> ingredients = new ComparableList<StockDto>();

                StockType type;
                while (await reader.ReadAsync())
                {
                    long id = Convert.ToInt64(reader.GetInt32(0));
                    StockType.TryParse(reader.GetString(1), out type);
                    int amount = reader.GetInt32(2);
                    var ingredient = new StockDto(type, amount, id);
                    ingredients.Add(ingredient);
                }
                return ingredients;
            }
            catch (Exception)
            {
                throw new PizzaException("The recipes ingredients cannot be found in the repository.");
            }
        }
    }
}
