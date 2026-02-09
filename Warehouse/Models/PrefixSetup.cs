using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models
{
    [Table("prefix_setup")]
    public class PrefixSetup
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_project_or_branch")]
        public int? IdProjectOrBranch { get; set; }

        [StringLength(7)]
        [Column("type")]
        public string? Type { get; set; }

        [StringLength(10)]
        [Column("prefix_req")]
        public string? PrefixReq { get; set; }

        [Column("consecutive_req")]
        public int? ConsecutiveReq { get; set; }

        [StringLength(7)]
        [Column("prefix_cotiz")]
        public string? PrefixCotiz { get; set; }

        [StringLength(7)]
        [Column("consecutive_cotiz")]
        public string? ConsecutiveCotiz { get; set; }

        [StringLength(7)]
        [Column("prefix_oc")]
        public string? PrefixOc { get; set; }

        [Column("consecutive_oc")]
        public int? ConsecutiveOc { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;
    }
}
