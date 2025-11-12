
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models
{
    [Table("proveedorxtablas")]
    public class ProveedorXTabla
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("id_tabla")]
        [DefaultValue(0)]
        public int? IdTabla { get; set; } = 0;

        [Column("campo1")]
        [DefaultValue(0)]
        public int? Campo1 { get; set; } = 0;

        [Column("campo2")]
        [StringLength(99)]
        [DefaultValue("NA")]
        public string? Campo2 { get; set; } = "NA";

        [Column("campo3")]
        [StringLength(99)]
        [DefaultValue("NA")]
        public string? Campo3 { get; set; } = "NA";

        [Column("campo4")]
        [StringLength(20)]
        [DefaultValue("NA")]
        public string? Campo4 { get; set; } = "NA";

        [Column("campo5")]
        [StringLength(99)]
        [DefaultValue("NA")]
        public string? Campo5 { get; set; } = "NA";

        [Column("campo6")]
        [StringLength(99)]
        [DefaultValue("NA")]
        public string? Campo6 { get; set; } = "NA";

        [Column("campo7")]
        [DefaultValue(false)]
        public bool Campo7 { get; set; } = false;

        [Column("campo8")]
        public DateTime? Campo8 { get; set; }

        [Column("campo9", TypeName = "DECIMAL(16,2)")]
        public decimal Campo9 { get; set; }

        [Column("campo10")]
        [DefaultValue(0)]
        public int? Campo10 { get; set; } = 0;

        [Column("parent_id")]
        [DefaultValue(0)]
        public int? IdParent { get; set; } = 0;

        [Column("sub_parent_id")]
        [DefaultValue(0)]
        public int? IdsubParent { get; set; } = 0;

        [Column("type")]
        [StringLength(10)]
        public string? Type { get; set; }

        [Column("vigente")]
        public bool Vigente { get; set; } = true;

        [Column("principal")]
        public bool Principal { get; set; } = true;

        [Column("active")]
        [DefaultValue(true)]
        public bool Active { get; set; } = true;
    }
}