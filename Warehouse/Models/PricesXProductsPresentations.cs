using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models;
[Table("pricesxproductspresentation")]
public class PricesXProductsPresentation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("id_materials")]
    public int IdMaterials { get; set; }

    [Required]
    [Column("id_catalogs")]
    public int IdCatalogs { get; set; }

    [Required]
    [Column("id_measures")]
    public int IdMeasures { get; set; }
    
    [StringLength(80)]
    [Column("description", TypeName = "NVARCHAR")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column("price", TypeName = "DECIMAL(18,2)")]
    public decimal Price { get; set; }

    [Required]
    [Column("active")]
    public bool Active { get; set; }
}