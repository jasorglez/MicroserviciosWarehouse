using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models;

[Table("materiaByCatalog")]
public class MateriaByCatalog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_company")]
    public int? IdCompany { get; set; }
    
    [Column("check")]
    public bool? Check { get; set; }
    
    [Column("id_concep")]
    public int? IdConcep { get; set; }
    
    [Column("id_catalog")]
    public int? IdCatalog { get; set; }
    

    [Column("costo_uni")]
    public decimal? CostoUni { get; set; }

    [Column("cantidad")]
    public decimal? Cantidad { get; set; }

    [Column("proporcion")]
    public decimal? Proporcion { get; set; }
    
    [Column("costo_tot")]
    public decimal? CostoTot { get; set; }
    
    [Column("merma")]
    public decimal? Merma { get; set; }

    [Column("costo_fin")]
    public decimal? CostoFin { get; set; }

    [Column("fecha_cambio")]
    public DateTime? FechaCambio { get; set; }

    [Column("parametros")]
    public int? Parametros { get; set; }
    
    [Column("total")]
    public decimal? Total { get; set; }

    [Column("active")]
    public bool Active { get; set; }
}