using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Views
{
    /// <summary>
    /// Modelo para la vista vw_ListProvidersForRawMaterial
    /// Lista proveedores asociados a materias primas
    /// </summary>
    [Table("vw_ListProvidersForRawMaterial", Schema = "dbo")]
    public class VwListProvidersForRawMaterial
    {
        [Column("id_provider")]
        public int IdProvider { get; set; }

        [Column("provider_name")]
        [StringLength(500)]
        public string ProviderName { get; set; }

        [Column("type_int_or_ext")]
        [StringLength(50)]
        public string TypeIntOrExt { get; set; }

        [Column("id_material")]
        public int IdMaterial { get; set; }

        [Column("active")]
        public bool Active { get; set; }
    }
}