using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    /// <summary>
    /// Gastos NO-materiales que deben aparecer en la Captura de Gastos y cuadrar en el
    /// reporte diario, pero que no entran por almacén (no generan entradas_molienda).
    /// Hoy: ANTICIPO de OC. A futuro: SERVICIO, NOMINA, IMPUESTO, etc.
    /// Es la fuente de verdad del "gasto" (montos, fechas, lo que se ve en Captura/Reporte).
    /// </summary>
    [Table("gastos_generales", Schema = "Delison")]
    public class GastosGeneralesDelison
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("id_company")]
        public int IdCompany { get; set; }

        [Column("id_branch")]
        public int IdBranch { get; set; }

        [Column("id_departament")]
        public int? IdDepartament { get; set; }

        // FK a ocandreq (para anticipos). NULL en nómina/servicios/impuestos sin OC.
        [Column("id_oc")]
        public int? IdOc { get; set; }

        // 'ANTICIPO' | 'SERVICIO' | 'NOMINA' | 'IMPUESTO' ...
        [Column("tipo_gasto", TypeName = "VARCHAR(20)")]
        [StringLength(20)]
        public string TipoGasto { get; set; } = string.Empty;

        // Folio OC SIN '-E1' (anticipo) o folio propio del gasto.
        [Column("folio", TypeName = "VARCHAR(255)")]
        [StringLength(255)]
        public string? Folio { get; set; }

        [Column("concepto", TypeName = "VARCHAR(200)")]
        [StringLength(200)]
        public string? Concepto { get; set; }

        [Column("id_provider")]
        public int? IdProvider { get; set; }

        [Column("proveedor", TypeName = "VARCHAR(150)")]
        [StringLength(150)]
        public string? Proveedor { get; set; }

        // % del anticipo (ej. 50). Solo display.
        [Column("porcentaje", TypeName = "decimal(7,2)")]
        public decimal? Porcentaje { get; set; }

        // VALOR del gasto (el monto del anticipo). Es la única cifra que importa para cuadrar.
        [Column("monto", TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        [Column("mas_iva")]
        public bool MasIva { get; set; } = false;

        // 'EN_TRAMITE' (en cola en Captura) | 'PAGADO' (ya pagado en Captura).
        [Column("estado", TypeName = "VARCHAR(15)")]
        [StringLength(15)]
        public string Estado { get; set; } = "EN_TRAMITE";

        // Fecha del clic "Registrar pago" en Nivel 3 (OC).
        [Column("fecha_registro")]
        public DateOnly FechaRegistro { get; set; }

        // Fecha del clic "Pagar" en Captura — la que CUENTA para el reporte diario.
        [Column("fecha_pago")]
        public DateOnly? FechaPago { get; set; }

        [Column("nota_factura", TypeName = "VARCHAR(100)")]
        [StringLength(100)]
        public string? NotaFactura { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;

        [Column("datemodified")]
        public DateTime DateModified { get; set; } = DateTime.Now;
    }
}
