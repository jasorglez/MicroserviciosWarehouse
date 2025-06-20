using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models;

[Table("rawmaterialdetails")]
public class RawMaterialDetails
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("id_rawmaterial")]
    public int IdRawMaterial { get; set; }
    
    [Column("id_material")]
    public int IdMaterial { get; set; }
    
    [Column("cost")]
    public decimal Cost { get; set; } = 0;
    
    [Column("quantity")]
    public decimal Quantity { get; set; } = 0;
    
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Column("total_cost")]
    public decimal TotalCost { get; set; } = 0;
    
    
    
    
}