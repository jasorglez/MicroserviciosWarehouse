using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    [Table("sucursalByMaterialProveedor", Schema = "Delison")]
    public class SucursalByMaterialProveedor
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_materialByProveedor")]
        public int? IdMaterialByProveedor { get; set; }

        [Column("id_sucursal")]
        public int? IdSucursal { get; set; }

        [Column("fecha_alta")]
        public DateTime? FechaAlta { get; set; }
        
        [Column("stock_minimo")]
        public int? StockMinimo { get; set; }

        [Column("resurtido")]
        public int? Resurtido { get; set; }

        [Column("capacidad_max_almacen")]
        public int? CapacidadMaxAlmacen { get; set; }

        [Column("tiempo_de_entrega")]
        public int? TiempoDeEntrega { get; set; }

        [Column("vigente")]
        public bool? Vigente { get; set; }

        [Column("active")]
        public bool? Active { get; set; }
    }
}