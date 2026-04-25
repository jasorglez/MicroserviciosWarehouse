using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models;

[Table("molienda")]
public class Molienda
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_company")]
    public int IdCompany { get; set; } = 1;

    [Column("id_sucursal")]
    public int? IdSucursal { get; set; }

    [Column("id_material")]
    public int? IdMaterial { get; set; }

    [Column("entradas")]
    public int Entradas { get; set; } = 0;

    [Column("salidas")]
    public int Salidas { get; set; } = 0;

    [Column("total_inventarios")]
    public int? TotalInventarios { get; set; } = 0;

    [Column("ajustes_inventarios")]
    public decimal? AjustesInventarios { get; set; } = 0;

    [Column("comentarios")]
    public string? Comentarios { get; set; }

    [Column("datemodified")]
    public DateTime DateModified { get; set; } = DateTime.Now;

    [Column("active")]
    public bool Active { get; set; } = true;
}
