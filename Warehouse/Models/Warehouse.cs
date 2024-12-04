using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Warehouse.Models
{


    [Table("warehouses")]
    public class Warehouset
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_businnes")]        
        public int IdBusinnes { get; set; }       

        [Required]
        [StringLength(50)]
        [Column("place", TypeName = "VARCHAR")]
        public string Place { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("name", TypeName = "VARCHAR")]
        [DefaultValue("NAME WAREHOUSES")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        [Column("address", TypeName = "NVARCHAR")]
        [DefaultValue("ADDRESS")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("state", TypeName = "VARCHAR")]
        public string State { get; set; } = string.Empty;

        [StringLength(40)]
        [Column("city", TypeName = "NCHAR")]
        public string? City { get; set; }

        
        [Required]
        [StringLength(10)]
        [Column("codepostal", TypeName = "CHAR")]
        [DefaultValue("CP")]
        public string CodePostal { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        [Column("phone", TypeName = "CHAR")]
        [DefaultValue("TELEFONO")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("leader", TypeName = "NVARCHAR")]
        [DefaultValue("LEADER")]
        public string Leader { get; set; } = string.Empty;

        [StringLength(2)]
        [Column("principal", TypeName = "VARCHAR")]
        public string? Principal { get; set; }

        [Required]
        [Column("active")]
        public bool Active { get; set; }
    }
}