using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [Column("id_supplie")]
        public int IdSupplie { get; set; }

        [Column("id_provider")]
        public int? IdProvider { get; set; }

        [Column("name_provider")]
        public string? NameProvider { get; set; }

        [Column("quantity", TypeName = "decimal(16,2)")]
        public decimal Quantity { get; set; }

        [Column("price", TypeName = "decimal(16,2)")]
        public decimal Price { get; set; }

        // Computed column in SQL
        [NotMapped]
        public decimal Total => Quantity * Price;

        [Column("dateuse", TypeName = "date")]
        public DateTime? Dateuse { get; set; }

        [Column("type")]
        [StringLength(6)]
        public string? Type { get; set; }

        [Column("recurrent")]
        [StringLength(10)]
        public string Recurrent { get; set; } = "Recurrente";

        [Column("namearticle")]
        [StringLength(50)]
        public string? NameArticle { get; set; }

        [Column("numarticle")]
        [StringLength(20)]
        public string? NumArticle { get; set; }

        [Column("intorext")]
        [StringLength(10)]
        public string IntOrExt { get; set; } = "Interno";

        [Column("provint")]
        [StringLength(50)]
        public string? ProvInt { get; set; }

        [Column("typepriority")]
        [StringLength(8)]
        public string? TypePriority { get; set; }

        [Column("pedimento")]
        public bool? Pedimento { get; set; }

        [Column("pedimentonum")]
        public int PedimentoNum { get; set; }

        [Column("comment")]
        public string? Comment { get; set; }

        [Column("active")]
        public bool? Active { get; set; } = true;
    }
}
