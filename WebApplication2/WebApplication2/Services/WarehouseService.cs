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
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IProduct_WarehouseRespository _productWarehouseRespository;
    private readonly IWarehouseRepository _warehouseRepository;

    public WarehouseService(IOrderRepository orderRepository, IProductRepository productRepository, IProduct_WarehouseRespository productWarehouseRespository, IWarehouseRepository warehouseRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _productWarehouseRespository = productWarehouseRespository;
        _warehouseRepository = warehouseRepository;
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
        
        if (dto.Amount <= 0)
        {
            throw new NotFoundException("Amount cannot be less than 0");
        }
        
        var order = await _orderRepository.GetOrderByProductIdAndAmountAsync(dto.IdProduct!.Value, dto.Amount);

        if (order == null)
        {
            throw new NotFoundException("Order not found");
        }
        
        if (order.CreatedAt > DateTime.UtcNow)
        {
            throw new BadRequestException("Order's creation date is in the future");
        }
        
        var productWarehouse  = await _productWarehouseRespository.GetProduct_WarehouseByIdOrderAsync(order.IdOrder);
        if (productWarehouse != null)
        {
            throw new BadRequestException("There already is order with this Id in table Product_Warehouse");
        }

        _orderRepository.UpdateFullfilledAtInOrder(order.IdOrder);

        Decimal price = order.Amount * product.Price;
        
        var idProductWarehouse = await _productWarehouseRespository.RegisterProductInProductWarehouseAsync(
            dto.IdWarehouse, dto.IdProduct, order.IdOrder, order.Amount, price, DateTime.UtcNow);

        if (!idProductWarehouse.HasValue)
            throw new Exception("Failed to register product in warehouse");

        return idProductWarehouse.Value;
        
    }
}