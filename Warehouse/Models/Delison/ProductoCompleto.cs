namespace Warehouse.Models.Delison
{

    public class ProductoCompleto
    {
        public int Id { get; set; }
        public string Producto { get; set; }
        public int? IdCompany { get; set; }  // Cambiado a nullable
        public decimal? Price { get; set; }  // Cambiado a nullable
        public bool? Vigente { get; set; }  // Cambiado a nullable
        public short? Active { get; set; }
    }
}