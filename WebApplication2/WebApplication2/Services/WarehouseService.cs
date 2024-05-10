using WebApplication2.Dto;
using WebApplication2.Exceptions;
using WebApplication2.Repositories;

namespace WebApplication2.Services;

public interface IWarehouseService
{
    public Task<int> RegisterProductInWarehouseAsync(RegisterProductInWarehouseRequestDTO dto);
}

public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;

    public WarehouseService(IWarehouseRepository warehouseRepository, IProductRepository productRepository, IOrderRepository orderRepository)
    {
        _warehouseRepository = warehouseRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
    }
    
    public async Task<int> RegisterProductInWarehouseAsync(RegisterProductInWarehouseRequestDTO dto)
    {
        var product = await _productRepository.GetProductByIdAsync(dto.IdProduct!.Value);
        if (product == null)
        {
            throw new NotFoundException("Product not found");
        }
        
        var warehouse = await _warehouseRepository.GetWarehouseByIdAsync(dto.IdWarehouse!.Value);
        if (warehouse == null)
        {
            throw new NotFoundException("Warehouse not found");
        }
        
        var order = await _orderRepository.GetOrderByIdAsync(dto.IdOrder);
        if (order == null)
        {
            throw new NotFoundException("Order not found");
        }
        
        if (order.CreatedAt > DateTime.UtcNow)
        {
            throw new BadRequestException("Order's creation date is in the future");
        }

        int idOrder = order.IdOrder;

        var idProductWarehouse = await _warehouseRepository.RegisterProductInWarehouseAsync(
            idWarehouse: dto.IdWarehouse!.Value,
            idProduct: dto.IdProduct!.Value,
            idOrder: idOrder,
            createdAt: DateTime.UtcNow);

        if (!idProductWarehouse.HasValue)
            throw new Exception("Failed to register product in warehouse");

        return idProductWarehouse.Value;
    }
}