using System.Data.SqlClient;
using WebApplication2.Models;

namespace WebApplication2.Repositories;

public interface IProductRepository
{
    public Task<Product> GetProductByIdAsync(int id);
} 
public class ProductRepository: IProductRepository
{
    private readonly IConfiguration _configuration;

    public ProductRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<Product> GetProductByIdAsync(int id)
    {
        using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await connection.OpenAsync();

        using var command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "SELECT IdProduct, Name, Description, Price FROM Product WHERE IdProduct = @IdProduct";
        command.Parameters.AddWithValue("@IdProduct", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Product
            {
                IdProduct = reader.GetInt32(reader.GetOrdinal("IdProduct")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price"))
            };
        }

        return null;
    }
}