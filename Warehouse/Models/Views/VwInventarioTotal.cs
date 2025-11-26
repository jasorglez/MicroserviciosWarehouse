using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Warehouse.Models.Views
{
    /// <summary>
    /// Modelo para la vista VW_InventarioTotal
    /// </summary>
    [Table("VW_InventarioTotal", Schema = "warehouses")]
    public class VwInventarioTotal
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("insumo")]
        [StringLength(100)]
        public string? Insumo { get; set; }

        [Column("description")]
        [StringLength(255)]
        public string? Description { get; set; }

        [Column("stockmin")]
        public int StockMin { get; set; }

        [Column("stockmax")]
        public int StockMax { get; set; }

        [Column("entrada")]
        public int Entrada { get; set; }

        [Column("salida")]
        public int Salida { get; set; }

        [Column("existencia")]
        public int Existencia { get; set; }

        [Column("ventaMN")]
        public decimal VentaMN { get; set; }

        [Column("total")]
        public decimal Total { get; set; }

        [Column("estado_stock")]
        [StringLength(50)]
        public string? EstadoStock { get; set; }

        [Column("id_company")]
        public int IdCompany { get; set; }
    }
}