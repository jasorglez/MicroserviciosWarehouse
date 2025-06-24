using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models;

[Table("rawmaterial")]
public class RawMaterial
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("id_company")]
    public int IdCompany { get; set; }
    
    [Column("id_material")]
    public int IdMaterial { get; set; }
    
    [Column("costo")]
    public decimal Costo { get; set; } = 0;
    
    [Column("cantidad")]
    public decimal Cantidad { get; set; } = 0;
    
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Column("costo_total")]
    public decimal CostoTotal { get; set; } = 0;
    
    [Column("merma")]
    public decimal Merma { get; set; } = 0;
    
    [Column("porcentaje_merma")]
    public decimal PorcentajeMerma { get; set; } = 0;
    
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Column("costo_final")]
    public decimal CostoFinal { get; set; } = 0;
    
    [Column("fecha_cambio")]
    public DateOnly FechaCambio { get; set; }
    
    [Column("active")]
    public bool Active { get; set; } = true;
}