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

    [Column("merma")]
    public decimal Merma { get; set; } = 0;
    
    [Column("fecha_cambio")]
    public DateOnly FechaCambio { get; set; }
    
    [Column("active")]
    public bool Active { get; set; } = true;
}