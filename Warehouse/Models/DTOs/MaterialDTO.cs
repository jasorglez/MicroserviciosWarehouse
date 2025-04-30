namespace Warehouse.Models.DTOs;

public class MaterialDTO
{
    public int Id { get; set; }
    public string? Insumo { get; set; }
    public string? Articulo { get; set; }
    public string? BarCode { get; set; }
    public string? Description { get; set; }
    public string Picture { get; set; } = string.Empty;
    public string TypeMaterial { get; set; } = string.Empty;
    public bool Vigente { get; set; }
    public bool Active { get; set; }
}