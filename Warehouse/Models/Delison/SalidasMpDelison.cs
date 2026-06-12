using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    // Salida / consumo de materia prima POR LOTE. Fuente que descuenta el inventario en vivo
    // y llena "Cantidad Salida" del detalle. Hoy origen = Molienda (MoliendaMatArticulo, Production).
    [Table("salidas_mp", Schema = "Delison")]
    public class SalidasMpDelison
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Lote-entrada consumido (datos_externos_molienda).
        [Column("id_dato_externo")]
        public int IdDatoExterno { get; set; }

        [Column("id_material")]
        public int IdMaterial { get; set; }

        [Column("cantidad", TypeName = "decimal(12,2)")]
        public decimal Cantidad { get; set; }

        [Column("fecha")]
        public DateOnly? Fecha { get; set; }

        // Nombre de quién utilizó (del Usuario de la molienda).
        [Column("usuario")]
        [MaxLength(100)]
        public string? Usuario { get; set; }

        // Id del consumo origen (MoliendaMatArticulo.id) — traza/edición. Cross-DB, sin FK.
        [Column("id_origen")]
        public int? IdOrigen { get; set; }

        [Column("tipo_origen")]
        [MaxLength(20)]
        public string? TipoOrigen { get; set; }   // 'MOLIENDA'

        [Column("active")]
        public bool Active { get; set; } = true;

        [Column("datemodified")]
        public DateTime DateModified { get; set; } = DateTime.UtcNow;
    }
}
