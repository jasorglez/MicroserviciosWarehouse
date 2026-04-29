using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace Warehouse.Models
{
    [Table("customer_loyalty_cards")]
    public class CustomerLoyaltyCard
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_loyalty_program")]
        public int IdLoyaltyProgram { get; set; }

        [Column("id_customer")]
        public int IdCustomer { get; set; }

        [Column("current_stamps")]
        [DefaultValue(0)]
        public int CurrentStamps { get; set; } = 0;

        [Column("total_rewards_earned")]
        [DefaultValue(0)]
        public int TotalRewardsEarned { get; set; } = 0;

        [Column("last_stamp_date")]
        public DateTime? LastStampDate { get; set; }
    }
}
