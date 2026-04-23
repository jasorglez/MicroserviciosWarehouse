using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    [Table("autorizacion_monto", Schema = "Delison")]
    public class AutorizacionMonto
    {
        [Key]
        public int Id { get; set; }

        [Column("id_company")]
        public int IdCompany { get; set; }

        [Column("nivel")]
        public byte Nivel { get; set; }

        [Column("monto_min", TypeName = "decimal(18,2)")]
        public decimal MontoMin { get; set; }

        [Column("monto_max", TypeName = "decimal(18,2)")]
        public decimal? MontoMax { get; set; }

        [Column("descripcion")]
        [StringLength(100)]
        public string? Descripcion { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;
    }
}
