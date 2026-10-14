using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models;

[Table("materialconversions")]
public class MaterialConversion
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_company")]
    public int IdCompany { get; set; }

    [Column("id_material")]
    public int IdMaterial { get; set; }

    [Column("id_measure_from")]
    public int IdMeasureFrom { get; set; }

    [Column("id_measure_to")]
    public int IdMeasureTo { get; set; }

    [Column("factor", TypeName = "decimal(18,6)")]
    public decimal Factor { get; set; }

    [Column("active")]
    public bool Active { get; set; } = true;
}

public class SaveMaterialConversionsRequest
{
    [Required]
    public int IdCompany { get; set; }
    public List<MaterialConversionRequest> Conversions { get; set; } = [];
}

public class MaterialConversionRequest
{
    [Range(1, int.MaxValue)]
    public int IdMeasureFrom { get; set; }
    [Range(1, int.MaxValue)]
    public int IdMeasureTo { get; set; }
    [Range(typeof(decimal), "0.000001", "999999999999.999999")]
    public decimal Factor { get; set; }
}

public class MaterialConversionResponse
{
    public int IdMaterial { get; set; }
    public int IdMeasureFrom { get; set; }
    public string MeasureFrom { get; set; } = string.Empty;
    public int IdMeasureTo { get; set; }
    public string MeasureTo { get; set; } = string.Empty;
    public decimal Factor { get; set; }
}
