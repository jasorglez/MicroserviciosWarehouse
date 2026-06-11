using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    // Nivel 5 "Datos externos" del Almacén de Molienda: lotes por entrada.
    // La suma de CantidadXLote de una entrada = entradas_molienda.cantidad_entrada.
    [Table("datos_externos_molienda", Schema = "Delison")]
    public class DatosExternosMoliendaDelison
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("id_entrada")]
        public int IdEntrada { get; set; }

        [Column("lote")]
        [MaxLength(50)]
        public string? Lote { get; set; }

        [Column("caducidad_meses")]
        public int? CaducidadMeses { get; set; }

        [Column("cantidad_x_lote", TypeName = "decimal(12,2)")]
        public decimal? CantidadXLote { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;

        [Column("datemodified")]
        public DateTime DateModified { get; set; } = DateTime.UtcNow;
    }
}
