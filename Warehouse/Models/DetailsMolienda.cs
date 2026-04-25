using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models;

[Table("detailsmolienda")]
public class DetailsMolienda
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_molienda")]
    public int IdMolienda { get; set; }

    [Column("type")]
    public string Type { get; set; } = "ENTRADA";

    [Column("fecha")]
    public DateOnly? Fecha { get; set; }

    [Column("cantidad")]
    public decimal Cantidad { get; set; } = 0;

    [Column("active")]
    public bool Active { get; set; } = true;
}
