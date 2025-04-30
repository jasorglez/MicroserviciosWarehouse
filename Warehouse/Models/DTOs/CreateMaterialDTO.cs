namespace Warehouse.Models.DTOs;

public class CreateMaterialDTO
{
    
    public int IdCompany { get; set; }
    
    public string? Insumo { get; set; }
    
    public string? Articulo { get; set; }
    
    public string? BarCode { get; set; }
    
    public int IdFamilia { get; set; }
    
    public int? IdSubfamilia { get; set; }
    
    public int IdMedida { get; set; }
    
    public int IdUbication { get; set; }
    
    public string? Description { get; set; }
    
    public DateTime? Date { get; set; }
    
    public bool? AplicaResg { get; set; }
    
    public decimal CostoMN { get; set; }
    
    public decimal CostoDLL { get; set; }
    
    public decimal VentaMN { get; set; }
    
    public decimal VentaDLL { get; set; }
    
    public int StockMin { get; set; }
    
    public int StockMax { get; set; }
    
    public string Picture { get; set; } = string.Empty;
    
    public string TypeMaterial { get; set; } = string.Empty;
    
    public bool Vigente { get; set; }
    
    public bool Active { get; set; }
}