
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Warehouse.Models
{
    [Table("inandout")]
    public class Inandout
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        

        [Column("folio", TypeName = "varchar(20)")]
        public string? Folio { get; set; }
        

        [Column("id_project")]
        public int? IdProject { get; set; }
        
        [Column("id_warehouse")]
        public int? IdWarehouse { get; set; }
        
        [Column("id_type")]
        public int? IdType { get; set; }

        [Column("id_ot")]
        public int? IdOt { get; set; }

        [Column("id_autoriza")]
        public int? IdAutoriza { get; set; }

        [Column("date", TypeName = "Date")]
        public DateTime? Date { get; set; }
        
        [Column("delivery_date", TypeName = "Date")]
        public DateTime? DeliveryDate { get; set; }
        
        [Column("id_oc")]
        public int? IdOc { get; set; }
        
        [Column("num_bill", TypeName = "varchar(30)")]
        public string? NumBill { get; set; }
        
        [Column("deliver_name", TypeName = "varchar(66)")]
        public string? DeliverName { get; set; }

        [Column("countrow")]
        public int? CountRow { get; set; }

        [Column("comment", TypeName = "varchar(200)")]
        public string? Comment { get; set; }
        
        [Column("type", TypeName = "varchar(10)")]
        public string? Type { get; set; }

        [Column("active", TypeName = "bit")]
        public bool? Active { get; set; } = true;
    }
}