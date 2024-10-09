using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Warehouse.Models
{

    [Table("catalog")]
    public class Catalog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(100)]
        [Column("description")]
        public string Description { get; set; } = "Description Catalog";

        [StringLength(25)]
        [Column("valueaddition")]
        public string ValueAddition { get; set; } = "NA";

        [StringLength(10)]
        [Column("type")]
        public string Type { get; set; } = "MEASURE";

        [Column("active")]
        public short Active { get; set; } = 1;
    }
}