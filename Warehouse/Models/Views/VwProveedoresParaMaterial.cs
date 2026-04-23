using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Views
{
    [Table("vw_ProveedoresParaMaterial", Schema = "dbo")]
    public class VwProveedoresParaMaterial
    {
        [Column("IdProvider")]
        public int IdProvider { get; set; }

        [Column("ProviderName")]
        [StringLength(500)]
        public string ProviderName { get; set; }

        [Column("TypeIntOrExt")]
        [StringLength(50)]
        public string TypeIntOrExt { get; set; }

        [Column("IdMaterial")]
        public int IdMaterial { get; set; }

        [Column("Active")]
        public bool Active { get; set; }
    }
}
