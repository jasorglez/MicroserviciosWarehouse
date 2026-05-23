
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Warehouse.Models
{
    [Table("configuration")]
    public class Configuration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("id_root")]
        public int IdRoot { get; set; }

        [Column("messagediary")]
        [DefaultValue(24)]
        public short? MessageDiary { get; set; }

        [Required]
        [Column("messagetime")]
        [DefaultValue(0)]
        public short MessageTime { get; set; }

        [Required]
        [StringLength(15)]
        [Column("prefixrequisition", TypeName = "VARCHAR")]
        [DefaultValue("RDM")]
        public string PrefixRequisition { get; set; } = "RDM";

        [Required]
        [Column("consecutivererequisition")]
        [DefaultValue(1)]
        public short ConsecutiveReRequisition { get; set; }

        [Required]
        [Column("numberrerequisition")]
        [DefaultValue(1)]
        public short NumberReRequisition { get; set; }

        [Required]
        [StringLength(15)]
        [Column("prefixorder", TypeName = "VARCHAR")]
        [DefaultValue("ODC")]
        public string PrefixOrder { get; set; } = "ODC";

        [Required]
        [Column("consecutiveorder")]
        [DefaultValue(1)]
        public short ConsecutiveOrder { get; set; }

        [Required]
        [Column("numberorder")]
        [DefaultValue(1)]
        public short NumberOrder { get; set; }

        [StringLength(15)]
        [Column("prefixint", TypeName = "VARCHAR")]
        [DefaultValue("VENT")]
        public string? PrefixInt { get; set; }

        [Required]
        [Column("consecutivint")]
        [DefaultValue(1)]
        public short ConsecutiveInt { get; set; }

        [Required]
        [Column("numberint")]
        [DefaultValue(1)]
        public short NumberInt { get; set; }

        [StringLength(15)]
        [Column("prefixout", TypeName = "VARCHAR")]
        [DefaultValue("VSAL")]
        public string? PrefixOut { get; set; }

        [Required]
        [Column("consecutiveout")]
        [DefaultValue(1)]
        public short ConsecutiveOut { get; set; }

        [Required]
        [Column("numberout")]
        [DefaultValue(1)]
        public short NumberOut { get; set; }

        [Column("active")]
        [DefaultValue(true)]
        public bool? Active { get; set; } = true;
    }
}
