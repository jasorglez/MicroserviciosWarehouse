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
        public int? IdArticulo { get; set; }   // nullable: hay filas históricas con id_articulo NULL (rompían EF al leer)

        [Column("editBultos")]
        public bool EditBultos { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Column("type")]
        public string Type { get; set; } = string.Empty;

        [Column("id_catalog")]
        public int? IdCatalog { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;

        [Column("molienda")]
        public bool Molienda { get; set; } = false;

        [Column("id_mat_prima")]
        public int? IdMatPrima { get; set; }

        [Column("id_prefijo_fase")]
        public int? IdPrefijoFase { get; set; }

        [Column("prefijo")]
        public string? Prefijo { get; set; }

        [Column("num_bote")]
        public int? NumBote { get; set; }

        [Column("id_branch")]
        public int? IdBranch { get; set; }
    }
}
