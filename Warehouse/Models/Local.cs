using System.Text.Json.Serialization;

namespace Warehouse.Models
{
    public class Local
    {
        [JsonPropertyName("id")]

        public int Id { get; set; }

        [JsonPropertyName("Name")]

        public string? Name { get; set; }
    }
}


