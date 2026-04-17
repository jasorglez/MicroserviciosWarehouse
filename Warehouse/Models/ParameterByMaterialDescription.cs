using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models;

[Table("parameterByMaterialDescription")]
public class ParameterByMaterialDescription
{
    [Key]
    [Column("id")]
    public int? Id { get; set; }
    
    [Column("id_master")]
    public int? IdMaster { get; set; }

    [Column("id_parameter")]
    public int? IdParameter { get; set; }

    [Column("minimo")]
    public decimal? Minimo { get; set; }

    [Column("objetivo")]
    public decimal? Objetivo { get; set; }

    [Column("maximo")]
    public decimal? Maximo { get; set; }

    [Column("vigente")]
    public bool? Vigente { get; set; }

    [Column("activo")]
    public bool? Activo { get; set; }
}