using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Warehouse.Models;

[Table("Detailsinandout")]

public class Detailsinandout
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Required]
    [Column("id_inandout")]
    public int IdInandout { get; set; }

    [Column("id_transfer_pair")]
    public int? IdTransferPair { get; set; }
    
    [Column("id_product")]
    public int? IdProduct { get; set; }

    [Column("id_parent_product")]
    public int? IdParentProduct { get; set; }

    [Column("component_name")]
    public string? ComponentName { get; set; }

    [Column("source_quantity", TypeName = "decimal(15,4)")]
    public decimal? SourceQuantity { get; set; }

    [Column("source_measure", TypeName = "nvarchar(30)")]
    public string? SourceMeasure { get; set; }

    [Column("source_measure_id")]
    public int? SourceMeasureId { get; set; }

    [Column("grams_per_meter_at_movement", TypeName = "decimal(15,4)")]
    public decimal? GramsPerMeterAtMovement { get; set; }

    [Column("conversion_factor_at_movement", TypeName = "decimal(18,6)")]
    public decimal? ConversionFactorAtMovement { get; set; }

    [Column("target_measure_at_movement", TypeName = "nvarchar(30)")]
    public string? TargetMeasureAtMovement { get; set; }

    [Column("target_measure_id_at_movement")]
    public int? TargetMeasureIdAtMovement { get; set; }
    
    [Column("quantity", TypeName = "decimal(9,2)")]
    public decimal? Quantity { get; set; }

    [Required]
    [Column("pending", TypeName = "decimal(9,2)")]
    public decimal Pending { get; set; }
    
    [Required]
    [Column("total", TypeName = "decimal(9,2)")]
    public decimal Total { get; set; }

    [Column("type")]
    public string? Type  { get; set; }

    [Column("active")] public bool Active { get; set; } = true;


}
