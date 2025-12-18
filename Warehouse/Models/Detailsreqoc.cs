
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

        
        [Column("id_movement")]
        public int IdMovement { get; set; }

        [Column("intorext")]
        public string Intorext { get; set; }
        
        [Column("id_supplie")]
        public int IdSupplie { get; set; }

        [Column("id_provider")]
        public int IdProvider { get; set; }

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
        public string? Type { get; set; }

        [Column("comment", TypeName = "VARCHAR")]
        public string? Comment { get; set; }
        
        [Column("typepriority")]
        public string TypePriority { get; set; }
        
        [Column("namearticle")]
        public string? NameArticle { get; set; }
        
        [Column("numarticle")]
        public string? NumArticle { get; set; }

        [Column("active")]
        public bool? Active { get; set; } = true;
    }
}