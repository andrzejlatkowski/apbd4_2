using System.Data.SqlClient;
using WebApplication2.Models;

namespace WebApplication2.Repositories;

public interface IOrderRepository
{
    public Task<Order> GetOrderByIdAsync(int? id);
}
public class OrderRepository :IOrderRepository
{
    private readonly IConfiguration _configuration;

    public OrderRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<Order> GetOrderByIdAsync(int? id)
    {
        using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await connection.OpenAsync();

        using var command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "SELECT IdOrder, CreatedAt FROM [Order] WHERE IdOrder = @IdOrder";
        command.Parameters.AddWithValue("@IdOrder", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Order
            {
                IdOrder = reader.GetInt32(reader.GetOrdinal("IdOrder")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }

        return null;
    }
}