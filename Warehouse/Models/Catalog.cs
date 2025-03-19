using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Warehouse.Models
{

    [Table("catalog")]
    public class Catalog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Column("id_company")]
        public int? IdCompany { get; set; }

        [StringLength(100)]
        [Column("description")]
        public string Description { get; set; } = "Description Catalog";

        [StringLength(25)]
        [Column("valueaddition")]
        public string? ValueAddition { get; set; } = "NA";

        [StringLength(25)]
        [Column("valueaddition2")]
        public string? ValueAddition2 { get; set; } = "NA";

        [Column("picture")]
        [StringLength(200)]
        public string Picture { get; set; } = "https://firebasestorage.googleapis.com/v0/b/beapp-501d1.appspot.com/o/images%2FBI.jpg?alt=media&token=2b73d977-5f18-4ae4-91a1-e715bf535f83";

        [Column("election")]
        public bool? IdElection { get; set; } = true;

        [StringLength(13)]
        [Column("type")]
        public string Type { get; set; } = "MEASURE";
        
        [Column("parent_id")]
        public int? ParentId { get; set; }

        [Column("active")]
        public short Active { get; set; } = 1;
    }
}