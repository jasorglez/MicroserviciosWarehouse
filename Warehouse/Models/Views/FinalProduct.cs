using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Warehouse.Models.Views
{
    /// <summary>
    /// Modelo para la vista vw_finalproduct
    /// </summary>
    [Table("vw_finalproduct", Schema = "dbo")]
    public class FinalProduct
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_company")]
        public int IdCompany { get; set; }

        [Column("presentation")]
        [StringLength(255)]
        public string Presentation { get; set; }

        [Column("category")]
        [StringLength(255)]
        public string Category { get; set; }

        [Column("flavor")]
        [StringLength(255)]
        public string Flavor { get; set; }

        [Column("parent_id")]
        public int? ParentId { get; set; }

        [Column("subparent_id")]
        public int? SubparentId { get; set; }

        [Column("active")]
        public short Active { get; set; }
    }
}