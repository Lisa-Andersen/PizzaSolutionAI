using Npgsql;
using PizzaPlace;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using System.Data;

namespace PizzaPlace.Repositories;

public class StockRepository : IStockRepository
{

    private static string stockTable = "stock_dto";
    private static readonly object _lock = new();

    /**
    * Get Add to Stock
    **/
    public async Task<StockDto> AddToStock(StockDto stock) {
        try
        {
            string request = $"UPDATE {stockTable} SET Amount=Amount+{stock.Amount} WHERE StockType='{stock.StockType}' RETURNING Id, Amount";
            return await ReadStockDtoFromDb(request, stock.StockType);
        }
        catch (Exception) {
            throw new PizzaException("The stock could not be updated.");
        }
    }

    /**
    * Get Stock
    **/
    public async Task<StockDto> GetStock(StockType stockType) {
        try
        {
            string request = $"SELECT Id, Amount FROM {stockTable} WHERE StockType='{stockType}'";
            return await ReadStockDtoFromDb(request, stockType);
        }
        catch (Exception) {
            throw new PizzaException("The stock could not be found.");
        }
    }

    /**
    * Take Stock
    **/
    public async Task<StockDto> TakeStock(StockType stockType, int amount) {
        try { 
            string request = $"UPDATE {stockTable} SET Amount=Amount-{amount} WHERE StockType='{stockType}' RETURNING Id, Amount";
            return await ReadStockDtoFromDb(request, stockType);
        }
        catch (Exception e) {
            throw new PizzaException("The stock could not be updated - did you attemt to remove take out more than exists in stock.");
        }   
    } 

    private async Task<StockDto> ReadStockDtoFromDb(string sqlRequest, StockType stockType) {
        long id = 0;
        int amount = 0;
        using (NpgsqlConnection connection = new NpgsqlConnection(Database.Database.ConnectionString))
        {
            await connection.OpenAsync();
            using NpgsqlCommand cmd = new NpgsqlCommand(sqlRequest, connection);
            NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                id = Convert.ToInt64(reader.GetInt32(0));
                amount = reader.GetInt32(1);
            }
        }


        StockDto stock = new StockDto(stockType, amount, id);
        return stock;
    }
}
