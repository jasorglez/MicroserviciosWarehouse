using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models
{
    [Table("materialxfinalproduct")]
    public class MaterialxFinalProduct
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int id { get; set; }
        
        [Column("id_material")]
        public int idMaterial { get; set; }
        
        [Column("id_presentation")]
        public int idPresentation { get; set; }
        
        [Column("active")]
        public bool Active { get; set; }
    }
}

