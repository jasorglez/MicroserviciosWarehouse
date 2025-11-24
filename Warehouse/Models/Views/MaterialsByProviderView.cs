using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Views
{
    /// <summary>
    /// Modelo para la vista vw_MaterialsByProviderAndBranch
    /// Muestra materiales asociados a proveedores con información de catálogos
    /// </summary>
    [Table("vw_MaterialsByProviderAndBranch", Schema = "dbo")]
    public class MaterialsByProviderView
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("id_provider")]
        public int IdProvider { get; set; }

        [Column("id_material")]
        public int? IdMaterial { get; set; }

        [Column("codigo")]
        [StringLength(100)]
        public string? Codigo { get; set; }

        [Column("nombre")]
        [StringLength(255)]
        public string? Nombre { get; set; }

        [Column("precio")]
        public decimal? Precio { get; set; }

        [Column("vigente")]
        public bool? Vigente { get; set; }

        [Column("active")]
        public bool Active { get; set; }

        [Column("idCategory")]
        public int? IdCategory { get; set; }

        [Column("categoria")]
        [StringLength(255)]
        public string? Categoria { get; set; }

        [Column("idFamilia")]
        public int? IdFamilia { get; set; }

        [Column("familia")]
        [StringLength(255)]
        public string? Familia { get; set; }

        [Column("idSubfamilia")]
        public int? IdSubfamilia { get; set; }

        [Column("subfamilia")]
        [StringLength(255)]
        public string? Subfamilia { get; set; }

        [Column("picture")]
        [StringLength(500)]
        public string? Picture { get; set; }
    }
}