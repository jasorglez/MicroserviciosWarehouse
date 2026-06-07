namespace Warehouse.Models.DTOs
{
    /// <summary>Una fila de peso/volumen (medida + unidad).</summary>
    public class EmpaquePesoVolumenItemDto
    {
        public decimal? Medida { get; set; }
        public int? IdUnidad { get; set; }
    }

    /// <summary>
    /// Guardado bulk del peso/volumen de UN par material-proveedor (proveedorxtablas.Id).
    /// Reemplaza por completo las filas existentes (en la práctica 0 o 1 fila).
    /// </summary>
    public class EmpaquePesoVolumenSaveDto
    {
        public int IdEmpaque { get; set; }
        public List<EmpaquePesoVolumenItemDto> Items { get; set; } = new();
    }
}
