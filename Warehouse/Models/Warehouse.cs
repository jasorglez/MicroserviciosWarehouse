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
        public int Id { get; set; }

        [Column("id_company")]
        [StringLength(20)]
        public string? IdCompany { get; set; }

        [Column("id_project")]
        [StringLength(20)]
        public string? IdProject { get; set; }

        [StringLength(50)]
        [Column("name", TypeName = "VARCHAR")]
        [DefaultValue("NAME WAREHOUSES")]
        public string Name { get; set; }

        [StringLength(150)]
        [Column("address", TypeName = "NVARCHAR")]
        [DefaultValue("ADDRESS")]
        public string Address { get; set; }

        [StringLength(40)]
        [Column("city", TypeName = "NCHAR")]
        public string City { get; set; }


        [Column("state", TypeName = "NVARCHAR")]
        public string State { get; set; }

        [StringLength(10)]
        [Column("codepostal", TypeName = "CHAR")]
        [DefaultValue("CP")]
        public string Cp { get; set; }

        [Column("place")]
        [StringLength(50)]
        public string Place { get; set; }

        [StringLength(10)]
        [Column("phone", TypeName = "CHAR")]
        [DefaultValue("TELEFONO")]
        public string Phone { get; set; }

        [StringLength(50)]
        [Column("leader", TypeName = "NVARCHAR")]
        [DefaultValue("LEADER")]
        public string Leader { get; set; }

        [StringLength(2)]
        [Column("principal", TypeName = "NVARCHAR")]
        public string? Principal { get; set; }
    }
}