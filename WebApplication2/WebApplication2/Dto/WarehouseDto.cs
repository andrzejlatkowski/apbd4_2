using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Dto;

public class RegisterProductInWarehouseRequestDTO
{
    [Required]
    public int? IdProduct { get; set; }
    
    [Required]
    public int? IdWarehouse { get; set; }
    [Required]
    public int? IdOrder { get; set; } 
    
    [Required]
    public DateTime? CreatedAt { get; set; }
}