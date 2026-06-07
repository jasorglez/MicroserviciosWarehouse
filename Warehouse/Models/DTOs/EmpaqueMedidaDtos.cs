namespace Warehouse.Models.DTOs
{
    /// <summary>Una fila de medida de empaque (medida + unidad + dimensión).</summary>
    public class EmpaqueMedidaItemDto
    {
        public decimal? Medida { get; set; }
        public int? IdUnidad { get; set; }
        public int? IdDimension { get; set; }
    }

    /// <summary>
    /// Guardado bulk de las medidas de UN par material-proveedor (proveedorxtablas.Id).
    /// Reemplaza por completo las filas existentes con la lista enviada (sin la fila vacía final).
    /// </summary>
    public class EmpaqueMedidaSaveDto
    {
        public int IdEmpaque { get; set; }
        public List<EmpaqueMedidaItemDto> Items { get; set; } = new();
    }
}
