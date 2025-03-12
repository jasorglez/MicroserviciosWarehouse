using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models
{
    [Table("setup")]
    public class Setup
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("id_company")]
        public int? IdCompany { get; set; }

        [StringLength(50)]
        [Column("description")]
        public string? Description { get; set; } = "Default Description";

        [Column("project_or_branch")]
        public bool? ProjectOrBranch { get; set; } = true;

        [Column("active")]
        public bool Active { get; set; } = true;
    }
}