namespace Warehouse.Models;

public class AjusteInventarioDto
{
    public int     IdMaterial      { get; set; }
    public int     IdWarehouse     { get; set; }
    public decimal CantidadFisica  { get; set; }
    public string? Comentario      { get; set; }
    public int     IdCompany       { get; set; }
}
