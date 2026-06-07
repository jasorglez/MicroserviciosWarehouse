using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    [Table("descripcion_empaque", Schema = "Delison")]
    public class DescripcionEmpaqueDelison
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("id_company")]
        public int IdCompany { get; set; }

        [Column("descripcion")]
        [StringLength(200)]
        public string Descripcion { get; set; } = string.Empty;

        [Column("active")]
        public bool Active { get; set; } = true;

        [Column("datemodified")]
        public DateTime DateModified { get; set; } = DateTime.Now;
    }
}
