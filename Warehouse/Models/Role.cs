using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models
{
    [Table("Roles", Schema = "dbo")]
    public class Role
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Column("id_company")]
        public int? IdCompany { get; set; }

        [Column("active")]
        public bool? Active { get; set; } = true;
    }
}
