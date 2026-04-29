using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace Warehouse.Models
{
    [Table("loyalty_programs")]
    public class LoyaltyProgram
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_company")]
        public int IdCompany { get; set; }

        [Column("name", TypeName = "VARCHAR(100)")]
        public string Name { get; set; } = "";

        [Column("id_product")]
        public int? IdProduct { get; set; }

        [Column("stamps_required")]
        [DefaultValue(5)]
        public int StampsRequired { get; set; } = 5;

        [Column("reward_description", TypeName = "VARCHAR(200)")]
        public string RewardDescription { get; set; } = "";

        [Column("active")]
        [DefaultValue(true)]
        public bool Active { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
