
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Views
{
    [Table("vw_MaterialsWithFamilies")] // Si quieres mapear explicitamente
    public class MaterialsWithFamiliesView
    {
        public int Id { get; set; }
        public string Insumo { get; set; }
        public string? MaterialDescription { get; set; }
        public string? FamiliaDescription { get; set; }
        public string? SubfamiliaDescription { get; set; }
        public string? Barcode { get; set; }
        public string? Picture { get; set; }
        public decimal? CostoMN { get; set; }
        public decimal? VentaMN { get; set; }
        public int? StockMin { get; set; }
        public int? StockMax { get; set; }
        public bool? Active { get; set; }
        public int IdCompany { get; set; }
    }
}