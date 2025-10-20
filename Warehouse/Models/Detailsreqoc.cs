
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Warehouse.Models
{
    [Table("detailsreqoc")]
    public class Detailsreqoc
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("id_movement")]
        public int IdMovement { get; set; }

        [Required]
        [Column("id_supplie")]
        public int IdSupplie { get; set; }
        
        [Required]
        [Column("dateuse", TypeName = "date")]
        public DateTime Dateuse { get; set; }

        [Required]
        [Column("quantity", TypeName = "DECIMAL(16,2)")]
        public decimal Quantity { get; set; }

        [Required]
        [Column("price", TypeName = "DECIMAL(16,2)")]
        public decimal Price { get; set; }               

        // Propiedad de solo lectura para calcular el total en el código si es necesario
        [NotMapped]
        public decimal Total => Quantity * Price;
    

        [Column("type", TypeName = "VARCHAR")]
        [StringLength(6)]
        public string? Type { get; set; }

        [Column("comment", TypeName = "VARCHAR")]
        [StringLength(20)]
        public string? Comment { get; set; }

        [Column("active")]
        public bool? Active { get; set; } = true;
    }
}