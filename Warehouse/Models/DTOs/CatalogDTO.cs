namespace Warehouse.Models.DTOs;

public class CatalogDTO
{
    public int Id { get; set; }
    public int? IdCompany { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ValueAddition { get; set; } = string.Empty;
    public string? ValueAddition2 { get; set; } = string.Empty;
    public bool? ValueAdditionBit { get; set; }
    public bool? ValueAdditionBit2 { get; set; }
    public bool? ValueAdditionBit3 { get; set; }
    public int? ParentId { get; set; }
    public int? SubParentId { get; set; }
    public decimal? Price { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool? Vigente { get; set; }
    public short Active { get; set; } = 1;
}