using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    [Table("masterFamily", Schema = "Delison")]
    public class MasterFamilyDelison
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_company")]
        public int? IdCompany { get; set; }

        [Column("master_Family")]
        public int? MasterFamily { get; set; }
        
        [Column("vigente")]
        public bool? Vigente { get; set; }

        [Column("active")]
        public bool? Active { get; set; }
    }
}