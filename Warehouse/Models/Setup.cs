using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models
{
    [Table("setup")]
    public class Setup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("id_company")]
        public int? IdCompany { get; set; }

        [StringLength(50)]
        [Column("description")]
        public string Description { get; set; } = "Default Description";

        [Column("idFiltrarProject")]
        public bool? IdProject { get; set; } = false;

        [Column("idFiltrarBranch")]
        public bool? IdBranch { get; set; } = false;

        [Column("idFiltrarCompany")]
        public bool? IdFiltrarCompany { get; set; } = false;

        [Column("active")]
        public bool Active { get; set; } = true;
    }
}