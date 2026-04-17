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
    
    [Required]
    [Column("id_product")]
    public int IdProduct { get; set; }
    
    [Column("quantity", TypeName = "decimal(9,2)")]
    public decimal? Quantity { get; set; }

    [Required]
    [Column("pending", TypeName = "decimal(9,2)")]
    public decimal Pending { get; set; }
    
    [Required]
    [Column("total", TypeName = "decimal(9,2)")]
    public decimal Total { get; set; }

    [Column("active")] public bool Active { get; set; } = true;


}