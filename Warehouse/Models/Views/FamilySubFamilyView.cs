using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Warehouse.Models.Views
{
    /// <summary>
    /// Modelo para la vista vw_family_subfamily_catalog
    /// </summary>
    [Table("vw_family_subfamily_catalog", Schema = "dbo")]
    public class FamilySubFamilyView
    {
        [Key]

        [Column("idCompany")]
        public int IdCompany { get; set; }

        [Column("idFamily")]
        public int? IdFamily { get; set; }

        [Column("familia")]
        public string? Familia { get; set; }

        [Column("idSubfamily")]
        public int? IdSubfamily { get; set; }

        [Column("subfamilia")]
        public string? Subfamilia { get; set; }

        [Column("status")]
        public bool? Status { get; set; }
    }
}