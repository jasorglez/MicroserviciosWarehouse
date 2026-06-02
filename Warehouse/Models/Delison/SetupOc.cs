using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    [Table("setup_oc")]
    public class SetupOc
    {
        [Key]
        public int Id { get; set; }

        [Column("id_company")]
        public int IdCompany { get; set; }

        [Column("id_branch")]
        public int IdBranch { get; set; }

        [Column("entrega_min")]
        public int EntregaMin { get; set; } = 1;

        [Column("entrega_max")]
        public int? EntregaMax { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;

        [Column("datemodified")]
        public DateTime DateModified { get; set; } = DateTime.Now;
    }
}
