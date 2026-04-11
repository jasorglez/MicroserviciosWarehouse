
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models
{
    [Table("subfamilyxprovider")]
    public class SubfamilyxProvider
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("id_subfamily")]
        [Required]
        public int IdSubfamily { get; set; }

        [Column("id_provider")]
        [Required]
        public int IdProvider { get; set; }

        [Column("vigente")]
        [DefaultValue(true)]
        public bool Vigente { get; set; } = true;

        [Column("principal")]
        [DefaultValue(false)]
        public bool Principal { get; set; } = false;

        [Column("active")]
        [DefaultValue(true)]
        public bool Active { get; set; } = true;
    }
}
