using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Warehouse.Models
{
    [Table("inandoutevidence")]
    public class Inandoutevidence
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("id_inandout")]
        public int IdInandout { get; set; }

        [Required]
        [StringLength(255)]
        [Column("image", TypeName = "VARCHAR")]
        public string Image { get; set; } = string.Empty;

        [StringLength(200)]
        [Column("comment", TypeName = "NVARCHAR")]
        public string? Comment { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("active")]
        public bool Active { get; set; } = true;
    }
}
