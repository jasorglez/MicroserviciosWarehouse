using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    [Table("CatalogByMasterFam", Schema = "Delison")]
    public class CatalogByMasterFamDelison
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_master")]
        public int? IdMaster { get; set; }

        [Column("id_catalog")]
        public int? IdCatalog { get; set; }
        
        [Column("vigente")]
        public bool? Vigente { get; set; }

        [Column("active")]
        public bool? Active { get; set; }
    }
}