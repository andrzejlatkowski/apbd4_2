using Microsoft.AspNetCore.Mvc;
using WebApplication2.Dto;
using WebApplication2.Repositories;


namespace WebApplication2.Controllers;

[ApiController]
[Route("/api/warehouses2")]
public class Warehouses2Controller : ControllerBase
{
    private readonly IProduct_WarehouseRespository _productWarehouseRespository;

    public Warehouses2Controller(IProduct_WarehouseRespository productWarehouseRespository)
    {
        _productWarehouseRespository = productWarehouseRespository;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddProductToWarehouse([FromBody] RegisterProductInWarehouseRequestDTO dto)
    {
        try
        {
            await _productWarehouseRespository.RegisterProductInWarehouseByProcedureAsync(dto.IdWarehouse, dto.IdProduct, dto.Amount, dto.CreatedAt);
        
            return Ok("Product added to warehouse successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}
