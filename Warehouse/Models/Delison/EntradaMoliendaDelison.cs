using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    [Table("entradas_molienda", Schema = "Delison")]
    public class EntradaMoliendaDelison
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("id_oc")]
        public int IdOc { get; set; }

        [Column("id_entrega")]
        public int? IdEntrega { get; set; }

        [Column("id_material")]
        public int? IdMaterial { get; set; }

        [Column("fecha_recepcion")]
        public DateOnly? FechaRecepcion { get; set; }

        [Column("cantidad_entrada", TypeName = "decimal(12,2)")]
        public decimal? CantidadEntrada { get; set; }

        [Column("bultos")]
        public int? Bultos { get; set; }

        [Column("revision_configu", TypeName = "decimal(12,2)")]
        public decimal? RevisionConfigu { get; set; }

        [Column("pago", TypeName = "decimal(12,2)")]
        public decimal? Pago { get; set; }

        // Fecha en que se confirmó/generó el pago real desde la Hoja de Gastos.
        [Column("fecha_pago")]
        public DateOnly? FechaPago { get; set; }

        // Nota / Factura por entrada. Se puede llenar desde la Hoja de Gastos si llegó vacía.
        [Column("nota_factura")]
        [MaxLength(20)]
        public string? NotaFactura { get; set; }

        // Número / folio del documento (nota o factura). Editable desde Captura de Gastos.
        [Column("num_nota_factura")]
        [MaxLength(50)]
        public string? NumNotaFactura { get; set; }

        [Column("usuario")]
        [MaxLength(100)]
        public string? Usuario { get; set; }

        [Column("liberacion")]
        public bool Liberacion { get; set; } = false;

        // Cierre por ENTRADA (lock). Marca que esta entrega quedó cerrada y no admite más cambios.
        // Aplica sobre todo a OCs "COMPRA AUTORIZADA SIN LIMITE" (entradas infinitas, cada una se cierra aparte).
        [Column("close")]
        public bool Close { get; set; } = false;

        [Column("active")]
        public bool Active { get; set; } = true;

        [Column("comentario")]
        [MaxLength(100)]
        public string? Comentario { get; set; }

        // Folio de entrega: folio OC + prefijo entrega + N (ej. OC-JIC3-P1-GON1429-E2)
        [Column("folio_entrega")]
        [MaxLength(50)]
        public string? FolioEntrega { get; set; }

        // Crédito: la entrada se ingresó "a crédito" (material disponible, pago pendiente a N días).
        // Separado de Liberacion (=pagado). Crédito activado → sigue en pendientes hasta que se paga.
        [Column("credito")]
        public bool Credito { get; set; } = false;

        // Fecha de vencimiento del crédito. Si NULL → se calcula en frontend (fechaRecepcion + N días).
        // Si tiene valor → el usuario la sobrescribió manualmente desde Captura de Gastos.
        [Column("fecha_vencimiento")]
        public DateOnly? FechaVencimiento { get; set; }

        // Monto del anticipo de la OC aplicado a ESTA entrada (FIFO o prorrateo).
        [Column("anticipo_aplicado", TypeName = "decimal(16,2)")]
        public decimal? AnticipoAplicado { get; set; }

        [Column("datemodified")]
        public DateTime DateModified { get; set; } = DateTime.UtcNow;
    }
}
