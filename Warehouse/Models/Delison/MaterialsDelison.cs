using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    [Table("materials", Schema = "Delison")]
    public class MaterialsDelison
    {
        [Key]
        public int Id { get; set; }

        [Column("article")]
        public string? Article { get; set; }

        [Column("id_categoria")]
        public int? CategoryId { get; set; }

        [Column("id_familia")]
        public int? FamilyId { get; set; }

        [Column("id_subfamilia")]
        public int? SubFamilyId { get; set; }

        [Column("price")]
        public decimal? Price { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("numPzaXPaq")]
        public int? NumPzaXPaq { get; set; }

        [Column("numMat")]
        public int? NumMat { get; set; }

        [Column("medidas")]
        public string? Medidas { get; set; }

        [Column("volume")]
        public string? Volume { get; set; }

        [Column("garantia")]
        public int? Garantia { get; set; }

        [Column("image")]
        public string? Image { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;
    }
}