namespace Warehouse.Models.DTOs;

public class PricesXProductsPresentationsDTO
{
    
    public int Id { get; set; }
    public int IdMaterials { get; set; }
    public int IdCatalogs { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool Active { get; set; }
    public CatalogDTO Catalog { get; set; }
}