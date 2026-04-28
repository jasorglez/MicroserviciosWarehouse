using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    [Table("molienda", Schema = "Delison")]
    public class MoliendaDelison
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("id_company")]
        public int IdCompany { get; set; }

        [Column("id_sucursal")]
        public int? IdSucursal { get; set; }

        [Column("id_material")]
        public int? IdMaterial { get; set; }

        [Column("entradas")]
        public int Entradas { get; set; } = 0;

        [Column("salidas")]
        public int Salidas { get; set; } = 0;

        [Column("total_inventarios")]
        public int? TotalInventarios { get; set; }

        [Column("ajustes_inventarios")]
        public decimal? AjustesInventarios { get; set; }

        [Column("total_entradas")]
        public decimal? TotalEntradas { get; set; }

        [Column("total_salidas")]
        public decimal? TotalSalidas { get; set; }

        [Column("type")]
        [MaxLength(15)]
        public string? Type { get; set; }

        [Column("comentarios")]
        [MaxLength(500)]
        public string? Comentarios { get; set; }

        [Column("datemodified")]
        public DateTime DateModified { get; set; } = DateTime.UtcNow;

        [Column("active")]
        public bool Active { get; set; } = true;
    }
}
