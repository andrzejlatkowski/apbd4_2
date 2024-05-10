using System.Data.SqlClient;
using WebApplication2.Models;

namespace WebApplication2.Repositories;

public interface IOrderRepository
{
    public Task<Order> GetOrderByProductIdAndAmountAsync(int idProduct, int amount);
    public void UpdateFullfilledAtInOrder(int order);
    
}
public class OrderRepository :IOrderRepository
{
    private readonly IConfiguration _configuration;

    public OrderRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<Order> GetOrderByProductIdAndAmountAsync(int idProduct, int amount)
    {
        using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await connection.OpenAsync();

        using var command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "SELECT IdOrder, IdProduct, Amount, CreatedAt FROM [Order] WHERE IdProduct = @IdProduct AND Amount = @Amount";
        command.Parameters.AddWithValue("@IdProduct", idProduct);
        command.Parameters.AddWithValue("@Amount", amount);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Order
            {
                IdOrder = reader.GetInt32(reader.GetOrdinal("IdOrder")),
                IdProduct = reader.GetInt32(reader.GetOrdinal("IdProduct")),
                Amount = reader.GetInt32(reader.GetOrdinal("Amount")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }

        return null;
    }
    
    public async void UpdateFullfilledAtInOrder(int idOrder)
    {
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await connection.OpenAsync();
        
        await using var transaction = await connection.BeginTransactionAsync();
        
        try
        {
            var updateQuery = "UPDATE [Order] SET FulfilledAt = @FulfilledAt WHERE IdOrder = @IdOrder";
            await using var updateCommand = new SqlCommand(updateQuery, connection);
            updateCommand.Transaction = (SqlTransaction)transaction;
            
            updateCommand.Parameters.AddWithValue("@IdOrder", idOrder);
            updateCommand.Parameters.AddWithValue("@FulfilledAt", DateTime.UtcNow);
            await updateCommand.ExecuteNonQueryAsync();
            
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw new Exception("Error updating FulfilledAt in Order");
        }
    }
}