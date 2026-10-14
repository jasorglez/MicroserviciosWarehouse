using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models;

[Table("materialcomponents")]
public class MaterialComponent
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_company")]
    public int IdCompany { get; set; }

    [Column("id_parent_material")]
    public int IdParentMaterial { get; set; }

    [Column("id_component_material")]
    public int IdComponentMaterial { get; set; }

    [Column("quantity", TypeName = "decimal(15,4)")]
    public decimal Quantity { get; set; }

    [Column("active")]
    public bool Active { get; set; } = true;
}

public class SaveMaterialComponentsRequest
{
    [Required]
    public int IdCompany { get; set; }

    public List<MaterialComponentRequest> Components { get; set; } = [];
}

public class MaterialComponentRequest
{
    [Required]
    public int IdComponentMaterial { get; set; }

    [Range(typeof(decimal), "0.0001", "999999999999.9999")]
    public decimal Quantity { get; set; }
}

public class MaterialComponentResponse
{
    public int IdComponentMaterial { get; set; }
    public decimal Quantity { get; set; }
    public string? Name { get; set; }
    public string? Measure { get; set; }
}
