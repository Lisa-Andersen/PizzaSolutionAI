using Npgsql;
using PizzaPlace.Models.Types;

namespace PizzaPlace.Database;

static class Database
{

    private static String Host = "localhost";
    private static int Port = 5432;
    private static String Username = "postgres";
    private static String Password = "pgAdmin";
    private static String DatabaseName = "PizzaPlace";

    public static string ConnectionString = $"Host={Host};Port={Port};Username={Username};Password={Password};Database={DatabaseName}";


    public static bool SqlRequest(String sql)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(Database.ConnectionString))
        {
            try
            {
                connection.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                {
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public static async Task<NpgsqlDataReader> SqlAsyncRequestResponse(String sql)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(Database.ConnectionString)) 
        {
            try
            {
                connection.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                {
                    return await cmd.ExecuteReaderAsync();
                }
            }
            catch (Exception)
            {
                throw new PizzaException("Something went wrong with the database.");
            }
        }
    }

    public static async Task<object?> SqlAsyncRequestObject(String sql)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(Database.ConnectionString))
        {
            try
            {
                connection.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                {
                    return await cmd.ExecuteScalarAsync();
                }
            }
            catch (Exception)
            {
                throw new PizzaException("Something went wrong with the database.");
            }
        }
    }


    public static void SetupTables()
    {
        CreateRecipeTable();
        CreateStockTable();
        CreateIngredientsTable();
        PopulateTables();
    }


    private static void CreateRecipeTable()
    {
        string dropTable = "DROP TABLE pizza_recipe CASCADE";
        SqlRequest(dropTable);

        // Create the pizza_recipe_dto table if it doesn't exist
        string createTable = "CREATE TABLE IF NOT EXISTS pizza_recipe (id SERIAL PRIMARY KEY, " +
                                                                          "RecipeType VARCHAR(60) UNIQUE NOT NULL, " +
                                                                          "CookingTime INT NOT NULL Check (CookingTime > 0))";
        SqlRequest(createTable);
    }

    private static void CreateIngredientsTable()
    {
        string dropTable = "DROP TABLE ingredients";
        SqlRequest(dropTable);

        // Create the pizza_recipe_dto table if it doesn't exist
        string createTable = "CREATE TABLE IF NOT EXISTS ingredients (id SERIAL PRIMARY KEY, " +
                                                                        "PizzaTypeId INT NOT NULL, " +
                                                                        "StockType VARCHAR(60) NOT NULL, " +
                                                                        "Amount INT NOT NULL CHECK (Amount > 0) DEFAULT 1," +
                                                                        "FOREIGN KEY (PizzaTypeId) REFERENCES pizza_recipe(Id))";
        SqlRequest(createTable);
    }

    private static void CreateStockTable()
    {
        string dropTable = "DROP TABLE stock_dto";
        SqlRequest(dropTable);

        // Create the pizza_recipe_dto table if it doesn't exist
        string createTable = "CREATE TABLE IF NOT EXISTS stock_dto (id SERIAL PRIMARY KEY, " +
                                                                    "StockType VARCHAR(60) UNIQUE NOT NULL, " +
                                                                    "Amount INT NOT NULL CHECK (Amount >= 0) DEFAULT 0)";
        SqlRequest(createTable);
    }

    // Intended for testing, we populate the tables.
    private static void PopulateTables() {
        // Create a recipe for recipe stock
        string insertStandardPizza = $"INSERT INTO pizza_recipe (RecipeType, CookingTime) VALUES ('{PizzaRecipeType.StandardPizza}', 1);";
        SqlRequest(insertStandardPizza);



        // Populate the ingredients table such that we can make a standard pizza
        Func<int> amountTimeGenerator = () => new Random().Next(1, 15); // Generate a random amount
        string insertSql = "INSERT INTO ingredients (PizzaTypeId, StockType, Amount) VALUES ";
        insertSql += $"(1, '{StockType.Bacon}', 1), ";
        insertSql += $"(1, '{StockType.Chocolate}', 1), ";
        insertSql += $"(1, '{StockType.Dough}', 1); ";
        SqlRequest(insertSql);


        // Populate the pizza_recipe_dto table with the enum values if they don't exist
        string insertStock = "INSERT INTO stock_dto (StockType, Amount) VALUES ";
        Func<int> amountGenerator = () => 100;// new Random().Next(20, 25); // Generate a random amount between 5 and 15 minutes
        foreach (var stockType in Enum.GetValues(typeof(StockType)))
        {
            insertStock += $"('{stockType}', {amountGenerator()}), ";
        }
        // Remove the trailing comma and space
        insertStock = insertStock.TrimEnd(',', ' ');
        SqlRequest(insertStock);
    }
}