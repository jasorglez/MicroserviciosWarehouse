using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    [Table("peso_volumen", Schema = "Delison")]
    public class PesoVolumenDelison
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("id_company")]
        public int IdCompany { get; set; }

        [Column("abreviatura")]
        [StringLength(20)]
        public string Abreviatura { get; set; } = string.Empty;

        [Column("nombre")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        // 'PESO' | 'VOLUMEN' — define la base de conversión (kg o L).
        [Column("tipo")]
        [StringLength(10)]
        public string? Tipo { get; set; }

        // Cuánto vale 1 unidad en la base (kg o L). Ej. ml=0.001, gal=3.78541, lb=0.453592.
        [Column("factor_base", TypeName = "decimal(18,8)")]
        public decimal? FactorBase { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;

        [Column("datemodified")]
        public DateTime DateModified { get; set; } = DateTime.Now;
    }
}
