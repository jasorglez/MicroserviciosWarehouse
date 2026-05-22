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

        [Column("usuario")]
        [MaxLength(100)]
        public string? Usuario { get; set; }

        [Column("liberacion")]
        public bool Liberacion { get; set; } = false;

        [Column("active")]
        public bool Active { get; set; } = true;

        [Column("comentario")]
        [MaxLength(100)]
        public string? Comentario { get; set; }

        [Column("datemodified")]
        public DateTime DateModified { get; set; } = DateTime.UtcNow;
    }
}
