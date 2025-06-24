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
    
    [Column("costo")]
    public decimal Costo { get; set; } = 0;
    
    [Column("cantidad")]
    public decimal Cantidad { get; set; } = 0;
    
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Column("costo_total")]
    public decimal? CostoTotal { get; set; } = 0;
    
    [Column("fecha_cambio")]
    public DateOnly? FechaCambio { get; set; }
    
    [Column("active")]
    public bool Active { get; set; } = true;
    
    
    
}