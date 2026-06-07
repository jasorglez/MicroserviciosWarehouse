namespace Warehouse.Models.DTOs
{
    /// <summary>Una presentación (descripción de empaque + pieza x paquete).</summary>
    public class EmpaqueDescripcionItemDto
    {
        public int? IdDescripcionEmpaque { get; set; }
        public int? PiezaXPaquete { get; set; }
    }

    /// <summary>
    /// Guardado bulk de las presentaciones de UN par material-proveedor (proveedorxtablas.Id).
    /// Reemplaza por completo las presentaciones (y en cascada las medidas/peso de las anteriores).
    /// Devuelve las filas insertadas CON sus ids (en el mismo orden de Items) para que el frontend
    /// guarde luego las medidas/peso de cada presentación por su id real.
    /// </summary>
    public class EmpaqueDescripcionSaveDto
    {
        public int IdProveedorTabla { get; set; }
        public List<EmpaqueDescripcionItemDto> Items { get; set; } = new();
    }

    // ── Agregación para el panel de Requisiciones ──────────────────────────────

    /// <summary>Una presentación con su tamaño (peso/volumen) ya convertido a base (kg o L).</summary>
    public class PresentacionItemDto
    {
        public int IdEmpaque { get; set; }
        public int? IdDescripcionEmpaque { get; set; }
        public string? DescripcionEmpaque { get; set; }
        public int? PiezaXPaquete { get; set; }
        public decimal? Medida { get; set; }        // en su unidad original
        public string? UnidadAbrev { get; set; }
        public string? Tipo { get; set; }           // 'PESO' | 'VOLUMEN'
        public decimal? FactorBase { get; set; }
        public decimal? MedidaBase { get; set; }    // Medida × FactorBase (kg o L)
    }

    /// <summary>Un proveedor del material con su compra mínima y sus presentaciones.</summary>
    public class ProveedorPresentacionesDto
    {
        public int IdProveedorTabla { get; set; }
        public int IdProvider { get; set; }
        public decimal MinCompra { get; set; }
        public List<PresentacionItemDto> Presentaciones { get; set; } = new();
    }
}
