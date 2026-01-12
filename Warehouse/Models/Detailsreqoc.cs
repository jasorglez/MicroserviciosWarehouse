
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
        public int IdSupplie { get; set; } = 0;

        [Column("id_provider")]
        public int? IdProvider { get; set; } = 0;

        [Column("name_provider")]
        public string? NameProvider { get; set; }

        [Column("quantity", TypeName = "decimal(16,2)")]
        public decimal Quantity { get; set; }

        [Column("price", TypeName = "decimal(16,2)")]
        public decimal Price { get; set; }

        // Columna calculada en SQL: ([quantity] * [price])
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("total", TypeName = "decimal(16,2)")]
        public decimal Total { get; private set; }

        [Column("dateuse", TypeName = "date")]
        public DateTime? Dateuse { get; set; }

        [Column("type", TypeName = "varchar(6)")]
        public string? Type { get; set; }

        [Column("recurrent", TypeName = "varchar(10)")]
        public string Recurrent { get; set; } = "Recurrente";

        [Column("namearticle", TypeName = "varchar(50)")]
        public string? NameArticle { get; set; }

        [Column("numarticle", TypeName = "varchar(20)")]
        public string? NumArticle { get; set; } = "numarticle";

        [Column("intorext", TypeName = "varchar(10)")]
        public string Intorext { get; set; } = "Interno";

        [Column("provint", TypeName = "varchar(50)")]
        public string? ProvInt { get; set; } = "Sin Proveedor";

        [Column("typepriority", TypeName = "varchar(8)")]
        public string? TypePriority { get; set; } = "Normal";

        [Column("pedimento")]
        public bool? Pedimento { get; set; } = false;

        [Column("pedimentonum")]
        public int PedimentoNum { get; set; } = 0;

        [Column("comment")]
        public string? Comment { get; set; }

        [Column("active")]
        public bool? Active { get; set; } = true;
    }
}
