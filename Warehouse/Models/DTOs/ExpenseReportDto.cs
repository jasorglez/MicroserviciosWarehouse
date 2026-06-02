namespace Warehouse.Models.DTOs
{
    /// <summary>
    /// Una celda del reporte gerencial de gastos: el cruce Sucursal × Departamento
    /// con el monto agregado en el periodo y lente seleccionados.
    /// </summary>
    public class ExpenseReportCellDto
    {
        public int IdBranch { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int IdDepartament { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int NumTransacciones { get; set; }
    }

    /// <summary>
    /// Respuesta del reporte gerencial de gastos. Devuelve las celdas ya agregadas
    /// (GROUP BY en SQL) para que el frontend solo pivote Sucursal × Departamento.
    /// Solo incluye celdas con gasto real (las sucursales/departamentos sin movimiento
    /// no aparecen, evitando filas vacías de sucursales basura).
    /// </summary>
    public class ExpenseReportDto
    {
        /// <summary>"PAGADO" (cash out, por fecha_recepcion) o "COMPROMETIDO" (por datecreate de OC).</summary>
        public string Lens { get; set; } = "PAGADO";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal GrandTotal { get; set; }
        public int TotalTransacciones { get; set; }
        public List<ExpenseReportCellDto> Cells { get; set; } = new();
    }
}
