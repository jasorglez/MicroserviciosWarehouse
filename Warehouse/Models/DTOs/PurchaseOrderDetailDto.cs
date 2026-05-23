namespace Warehouse.Models.DTOs
{
    public class PurchaseOrderDetail
    {
        public int Id { get; set; }
        public string Folio { get; set; }
        public DateTime DateCreate { get; set; }
        public int? IdSupplie { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public int? IdProvider { get; set; }
        public decimal TotalPrice => Quantity * Price; // Calculated property
    }
}


