namespace Warehouse.Models.DTOs;

public class CreatePriceXProductsPresentationDTO
{
    public int IdMaterials { get; set; }
    
    public int IdCatalogs { get; set; }
    
    public string Description { get; set; } = string.Empty;
    
    public decimal Price { get; set; }
    
    public bool Active { get; set; }
}