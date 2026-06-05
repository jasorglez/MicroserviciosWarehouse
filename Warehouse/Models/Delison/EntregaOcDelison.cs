using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    [Table("entregas_oc", Schema = "Delison")]
    public class EntregaOcDelison
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("id_detailsreqoc")]
        public int IdDetailsreqoc { get; set; }

        [Column("fecha_entrega")]
        public DateOnly? FechaEntrega { get; set; }

        [Column("cantidad_recibir", TypeName = "decimal(16,2)")]
        public decimal? CantidadRecibir { get; set; }

        [Column("nota_factura")]
        [MaxLength(20)]
        public string? NotaFactura { get; set; }

        [Column("total_entrega", TypeName = "decimal(16,2)")]
        public decimal? TotalEntrega { get; set; }

        [Column("fecha_entrada_almacen")]
        public DateOnly? FechaEntradaAlmacen { get; set; }

        [Column("close")]
        public bool Close { get; set; } = false;

        // IVA propio de ESTA entrega (independiente del ítem OC). Permite que en multi-entrega
        // cada entrega lleve o no IVA por separado. Se fija al pagar la entrega en la Hoja de Gastos.
        [Column("mas_iva")]
        public bool MasIva { get; set; } = false;

        [Column("active")]
        public bool Active { get; set; } = true;

        [Column("datemodified")]
        public DateTime DateModified { get; set; } = DateTime.UtcNow;
    }
}
