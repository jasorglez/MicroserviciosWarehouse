namespace Warehouse.Models.DTOs
{
    public class FrequentArticlesResponse
    {
        public List<FrequentArticleDto> Articles { get; set; } = new List<FrequentArticleDto>();
        public int TotalRequisitions { get; set; }
    }
}
