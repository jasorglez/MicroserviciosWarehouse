using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Warehouse.Models.Views
{
    /// <summary>
    /// Modelo para la vista vw_MaterialsWithCounts
    /// </summary>
    [Table("vw_MaterialsWithCounts", Schema = "dbo")]
    public class MaterialWithCount
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("insumo")]
        [StringLength(100)]
        public string? Insumo { get; set; }

        [Column("articulo")]
        [StringLength(255)]
        public string? Articulo { get; set; }

        [Column("id_category")]
        public int IdCategory { get; set; }

        [Column("categoria")]
        [StringLength(255)]
        public string? Categoria { get; set; }

        [Column("id_familia")]
        public int? IdFamilia { get; set; }

        [Column("familia")]
        [StringLength(255)]
        public string? Familia { get; set; }

        [Column("id_subfamilia")]
        public int IdSubfamilia { get; set; }

        [Column("subfamilia")]
        [StringLength(255)]
        public string? Subfamilia { get; set; }

        [Column("picture")]
        [StringLength(250)]
        public string? Picture { get; set; }

        [Column("provider_count")]
        public int? ProviderCount { get; set; }

        [Column("subfamily_count")]
        public int? SubfamilyCount { get; set; }

        [Column("costos_count")]
        public decimal? Costo { get; set; }

        [Column("id_company")]
        public int IdCompany { get; set; }
        
        [Column("vigente")]
        public bool? Vigente { get; set; }
    }
}