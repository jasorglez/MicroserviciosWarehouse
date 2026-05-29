using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    [Table("extractionfermentationcatalog", Schema = "Delison")]
    public class ExtractionFermentationCatalog
    {
        [Key]
        public int Id { get; set; }

        [Column("id_company")]
        public int IdCompany { get; set; }

        [Column("id_branch")]
        public int? IdBranch { get; set; }

        [Column("description")]
        [StringLength(150)]
        public string Description { get; set; } = string.Empty;

        [Column("active")]
        public bool Active { get; set; } = true;

        [Column("molienda")]
        public bool Molienda { get; set; } = false;
    }
}
