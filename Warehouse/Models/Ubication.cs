using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models
{
    [Table("ubication", Schema = "bi2")]
    public class Ubication
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("description", TypeName = "VARCHAR(100)")]
        [StringLength(100)]
        public string? Description { get; set; }

        [Column("comment", TypeName = "VARCHAR(200)")]
        [StringLength(200)]
        public string? Comment { get; set; }
    }
}
