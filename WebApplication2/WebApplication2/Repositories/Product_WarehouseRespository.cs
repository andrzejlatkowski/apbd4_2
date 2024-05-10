using System.Data;
using System.Data.SqlClient;
using WebApplication2.Models;

namespace WebApplication2.Repositories;

public interface IProduct_WarehouseRespository
{
    public Task<ProductWarehouse> GetProduct_WarehouseByIdOrderAsync(int orderId);
    public Task<int?> RegisterProductInProductWarehouseAsync(int? idWarehouse, int? idProduct, int? idOrder, int amount, Decimal price, DateTime createdAt);
    public Task<int?> RegisterProductInWarehouseByProcedureAsync(int? idWarehouse, int? idProduct, int amount, DateTime createdAt);
}
public class Product_WarehouseRespository: IProduct_WarehouseRespository
{
    private readonly IConfiguration _configuration;

    public Product_WarehouseRespository(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    
    public async Task<ProductWarehouse> GetProduct_WarehouseByIdOrderAsync(int orderId)
    {
        using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await connection.OpenAsync();

        using var command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "SELECT IdProductWarehouse FROM Product_Warehouse WHERE IdOrder = @IdOrder";
        command.Parameters.AddWithValue("@IdOrder", orderId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new ProductWarehouse
            {
                IdProductWarehouse = reader.GetInt32(reader.GetOrdinal("IdProductWarehouse")),
            };
        }
        return null;
    }

    public async Task<int?> RegisterProductInProductWarehouseAsync(int? idWarehouse, int? idProduct, int? idOrder, int amount, Decimal price, DateTime createdAt)
    {
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await connection.OpenAsync();
        
        await using var transaction = await connection.BeginTransactionAsync();
        
        try
        {
            var query = @"
                      INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                      OUTPUT Inserted.IdProductWarehouse
                      VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt);";
            await using var command = new SqlCommand(query, connection);
            command.Transaction = (SqlTransaction)transaction;
            
            command.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
            command.Parameters.AddWithValue("@IdProduct", idProduct);
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.Parameters.AddWithValue("@Amount", amount);
            command.Parameters.AddWithValue("@Price", price);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
            
            var idProductWarehouse = (int)await command.ExecuteScalarAsync();

            await transaction.CommitAsync();
            return idProductWarehouse;
        }
        catch
        {
            
            await transaction.RollbackAsync();
            throw new Exception("Error registering product in Product_Warehouse");
        }
    }
    
    public async Task<int?> RegisterProductInWarehouseByProcedureAsync(int? idWarehouse, int? idProduct, int amount, DateTime createdAt)
    {
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await connection.OpenAsync();
    
        await using var transaction = await connection.BeginTransactionAsync();
    
        try
        {
            await using var command = new SqlCommand("AddProductToWarehouse", connection);
            command.Transaction = (SqlTransaction)transaction;
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
            command.Parameters.AddWithValue("@IdProduct", idProduct);
            command.Parameters.AddWithValue("@Amount", amount);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
            await command.ExecuteNonQueryAsync();

            // No need to call ExecuteScalarAsync here
        
            await transaction.CommitAsync();
            return null; // Or return any relevant value
        }
        catch
        {
            await transaction.RollbackAsync();
            throw new Exception("Error registering product in Product_Warehouse");
        }
    }
}
