using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models;

[Table("materialbrands")]
public class MaterialBrand
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("id_material")]
    public int IdMaterial { get; set; }

    [Required]
    [Column("id_catalog")]
    public int IdCatalog { get; set; }

    [Column("active")]
    public bool Active { get; set; } = true;
}

public class SaveMaterialBrandsRequest
{
    public int IdCompany { get; set; }
    public List<int> CatalogIds { get; set; } = new();
}
