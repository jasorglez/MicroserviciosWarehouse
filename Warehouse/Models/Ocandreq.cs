
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Warehouse.Models
{
    [Table("ocandreq")]
    public class Ocandreq
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("folio", TypeName = "VARCHAR")]
        [StringLength(20)]
        public string Folio { get; set; } = string.Empty;

        [Column("id_project")]
        public int? IdProject { get; set; }

        [Required]
        [Column("datecreate", TypeName = "DATE")]
        public DateTime DateCreate { get; set; }
        
        [Column("id_proveedor")]
        [StringLength(20)]
        public int IdProveedor { get; set; }

        [Required]
        [Column("id_departament")]
        public int IdDepartament { get; set; }

        [Required]
        [Column("delivery", TypeName = "VARCHAR")]
        [StringLength(80)]
        public string Delivery { get; set; } = string.Empty;

        [Required]
        [Column("deliverytime", TypeName = "VARCHAR")]
        [StringLength(20)]
        public string DeliveryTime { get; set; } = string.Empty;

        [Required]
        [Column("typeoc", TypeName = "VARCHAR")]
        [StringLength(20)]
        public string TypeOc { get; set; } = string.Empty;

        [Required]
        [Column("datesupply", TypeName = "DATE")]
        public DateTime DateSupply { get; set; }

        [Required]
        [Column("id_payment")]
        public int IdPayment { get; set; }

        [Required]
        [Column("id_currency")]
        public int IdCurrency { get; set; }

        [Column("conditions", TypeName = "VARCHAR")]
        [StringLength(50)]
        public string? Conditions { get; set; }

        [Column("id_authorize", TypeName = "VARCHAR")]
        [StringLength(30)]
        public string? IdAuthorize { get; set; }

        [Column("priority", TypeName = "VARCHAR")]
        [StringLength(12)]
        public string? Priority { get; set; }

        [Column("solicit", TypeName = "VARCHAR")]
        [StringLength(50)]
        public string? Solicit { get; set; }

        [Column("type", TypeName = "VARCHAR")]
        [StringLength(6)]
        public string? Type { get; set; }

        [Column("comments", TypeName = "VARCHAR")]
        [StringLength(50)]
        public string? Comments { get; set; }

        [Column("active")]
        public bool? Active { get; set; } = true;
    }
}