using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    [Table("materialxmodulo", Schema = "Delison")]
    public class MaterialXModulo
    {
        [Key]
        public int Id { get; set; }

        [Column("id_company")]
        public int IdCompany { get; set; }

        [Column("id_articulo")]
        public int IdArticulo { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Column("type")]
        public string Type { get; set; } = string.Empty;

        [Column("id_catalog")]
        public int? IdCatalog { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;
    }
}
