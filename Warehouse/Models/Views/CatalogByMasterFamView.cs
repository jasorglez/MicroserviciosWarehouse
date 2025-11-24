using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Warehouse.Models.Views
{
    /// <summary>
    /// Modelo para la vista vw_family_subfamily_catalog
    /// </summary>
    [Table("CatalogByMasterFamView", Schema = "dbo")]
    public class CatalogByMasterFamView
    {
        [Key]

        [Column("IdMaster")]
        public int IdMaster { get; set; }

        [Column("MasterFamily")]
        public int MasterFamily { get; set; }

        [Column("IdCompanyMaster")]
        public int IdCompanyMaster { get; set; }

        [Column("idFamily")]
        public int? IdFamily { get; set; }

        [Column("familia")]
        public string? Familia { get; set; }

        [Column("idSubfamily")]
        public int? IdSubfamily { get; set; }

        [Column("subfamilia")]
        public string? Subfamilia { get; set; }

        [Column("vigente")]
        public bool? Vigente { get; set; }
    }
}