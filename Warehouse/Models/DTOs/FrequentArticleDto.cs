namespace Warehouse.Models.DTOs
{
    public class FrequentArticleDto
    {
        public int IdSupplie { get; set; }
        public string NameArticle { get; set; }
        public int CountRequested { get; set; }
        public decimal TotalQuantity { get; set; }
    }
}
