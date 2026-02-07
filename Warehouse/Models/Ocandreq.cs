
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Warehouse.Models
{
    [Table("ocandreq")]
    public class Ocandreq
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("folio", TypeName = "VARCHAR(255)")]
        [StringLength(255)]
        public string Folio { get; set; } = string.Empty;

        [Required]
        [Column("type_reference", TypeName = "VARCHAR(50)")]
        [StringLength(50)]
        public string TypeReference { get; set; } = "project";

        [Column("id_req")]
        public int? IdReq { get; set; } = 0;

        [Column("id_reference")]
        public int IdReference { get; set; } = 0;

        [Required]
        [Column("datecreate", TypeName = "DATE")]
        public DateTime DateCreate { get; set; }

        [Column("id_provider")]
        public int? IdProvider { get; set; } = 0;

        [Required]
        [Column("id_departament")]
        public int IdDepartament { get; set; } = 0;

        [Required]
        [Column("delivery", TypeName = "VARCHAR(80)")]
        [StringLength(80)]
        public string Delivery { get; set; } = "NO APLICA";

        [Required]
        [Column("deliverytime", TypeName = "VARCHAR(20)")]
        [StringLength(20)]
        public string? DeliveryTime { get; set; } = "1 DAY";

        [Required]
        [Column("typeoc", TypeName = "VARCHAR(20)")]
        [StringLength(20)]
        public string TypeOc { get; set; } = "INSUMOS";

        [Column("datesupply", TypeName = "DATE")]
        public DateTime? DateSupply { get; set; }

        [Required]
        [Column("id_payment")]
        public int IdPayment { get; set; } = 0;

        [Required]
        [Column("id_currency")]
        public int IdCurrency { get; set; } = 0;

        [Column("conditions", TypeName = "VARCHAR(50)")]
        [StringLength(50)]
        public string? Conditions { get; set; }

        [Column("id_authorize")]
        public int? IdAuthorize { get; set; } = 0;

        [Column("priority", TypeName = "VARCHAR(12)")]
        [StringLength(12)]
        public string? Priority { get; set; }

        [Column("solicit", TypeName = "VARCHAR(50)")]
        [StringLength(50)]
        public string? Solicit { get; set; }

        [Column("discount", TypeName = "DECIMAL(38,2)")]
        public decimal? Discount { get; set; } = 0;

        [Column("iva_retention", TypeName = "DECIMAL(38,2)")]
        public decimal? IvaRetention { get; set; } = 0;

        [Column("id_solicit")]
        public int? IdSolicit { get; set; } = 0;

        [Column("address", TypeName = "VARCHAR(200)")]
        [StringLength(200)]
        public string? Address { get; set; }

        [Column("city", TypeName = "VARCHAR(100)")]
        [StringLength(100)]
        public string? City { get; set; }

        [Column("phone", TypeName = "VARCHAR(10)")]
        [StringLength(10)]
        public string? Phone { get; set; }

        [Column("type", TypeName = "VARCHAR(6)")]
        [StringLength(6)]
        public string? Type { get; set; } = "OC";

        [Column("pedimento")]
        public int? Pedimento { get; set; } = 0;

        [Column("compliancepedimento", TypeName = "DECIMAL(7,2)")]
        public decimal? CompliancePedimento { get; set; }

        [Column("compliancerequesicion", TypeName = "DECIMAL(7,2)")]
        public decimal? ComplianceRequesicion { get; set; }

        [Column("comments", TypeName = "VARCHAR(50)")]
        [StringLength(50)]
        public string? Comments { get; set; }

        [Column("close")]
        public bool? Close { get; set; } = false;
        
        [Column("countitem")]
        public int? CountItem { get; set; }
        
        [Column("locked")]
        public bool? Locked { get; set; } = false;

        [Column("active")]
        public bool? Active { get; set; } = true;
    }
}