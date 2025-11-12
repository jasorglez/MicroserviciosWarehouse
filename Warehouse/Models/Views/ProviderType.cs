using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Views
{
    [Table("vw_providerxtype", Schema = "dbo")]
    public class ProviderType
    {
        [Column("id")]
        public int Id { get; set; }
        
        [Column("id_provider")]
        public int IdProvider { get; set; }
        
        [Column("id_parent")]
        public int IdParent { get; set; }
        
        [Column("id_subparent")]
        public int IdSubparent { get; set; }
        
        [Column("name_parent")]
        public string? NameParent { get; set; }
        
        [Column("name_subparent")]
        public string? NameSubparent { get; set; }
        
        [Column("name_product")]
        public string? NameProduct { get; set; }
        
        [Column("vigente")]
        public bool Vigente { get; set; }
        
        [Column("principal")]
        public bool Principal { get; set; }
        
        [Column("type")]
        public string? Type { get; set; }
        
        [Column("active")]
        public bool Active { get; set; }
    }
}