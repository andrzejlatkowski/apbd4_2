using System.Data;
using System.Data.SqlClient;
using WebApplication2.Models;

namespace WebApplication2.Repositories;

public interface IWarehouseRepository
{
    public Task<Warehouse> GetWarehouseByIdAsync(int idWarehouse);
    public Task<int?> RegisterProductInWarehouseAsync(int idWarehouse, int idProduct, int idOrder, DateTime createdAt);
    public Task RegisterProductInWarehouseByProcedureAsync(int idWarehouse, int idProduct, DateTime createdAt);
}

public class WarehouseRepository : IWarehouseRepository
{
    private readonly IConfiguration _configuration;
    public WarehouseRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public async Task<Warehouse> GetWarehouseByIdAsync(int idWarehouse)
    {
        using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await connection.OpenAsync();

        using var command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "SELECT IdWarehouse, Name, Address FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
        command.Parameters.AddWithValue("@IdWarehouse", idWarehouse);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Warehouse
            {
                IdWarehouse = reader.GetInt32(reader.GetOrdinal("IdWarehouse")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Address = reader.GetString(reader.GetOrdinal("Address"))
            };
        }

        return null;
    }
    
    public async Task<int?> RegisterProductInWarehouseAsync(int idWarehouse, int idProduct, int idOrder, DateTime createdAt)
    {
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await connection.OpenAsync();
        
        await using var transaction = await connection.BeginTransactionAsync();
        
        try
        {
            var query = "UPDATE \"Order\" SET FulfilledAt = @FulfilledAt WHERE IdOrder = @IdOrder";
            await using var command = new SqlCommand(query, connection);
            command.Transaction = (SqlTransaction)transaction;
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.Parameters.AddWithValue("@FulfilledAt", DateTime.UtcNow);
            await command.ExecuteNonQueryAsync();
            
            command.CommandText = @"
                      INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, CreatedAt, Amount, Price)
                      OUTPUT Inserted.IdProductWarehouse
                      VALUES (@IdWarehouse, @IdProduct, @IdOrder, @CreatedAt, 0, 0);";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
            command.Parameters.AddWithValue("@IdProduct", idProduct);
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.Parameters.AddWithValue("@CreatedAt", createdAt);
            var idProductWarehouse = (int)await command.ExecuteScalarAsync();

            await transaction.CommitAsync();
            return idProductWarehouse;
        }
        catch
        {
            await transaction.RollbackAsync();
            return null;
        }
    }
    
    public async Task RegisterProductInWarehouseByProcedureAsync(int idWarehouse, int idProduct, DateTime createdAt)
    {
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await connection.OpenAsync();
        
        await using var command = new SqlCommand("AddProductToWarehouse", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("IdProduct", idProduct);
        command.Parameters.AddWithValue("IdWarehouse",idWarehouse);
        command.Parameters.AddWithValue("Amount", 0);
        command.Parameters.AddWithValue("CreatedAt", createdAt);
        await command.ExecuteNonQueryAsync();
    }
}