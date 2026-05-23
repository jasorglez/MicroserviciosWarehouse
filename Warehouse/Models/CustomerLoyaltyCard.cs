using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models;

[Table("customer_loyalty_cards")]
public class CustomerLoyaltyCard
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_loyalty_program")]
    public int IdLoyaltyProgram { get; set; }

    [Column("id_customer")]
    public int IdCustomer { get; set; }

    [Column("current_stamps")]
    public int CurrentStamps { get; set; }

    [Column("total_rewards_earned")]
    public int TotalRewardsEarned { get; set; }

    [Column("last_stamp_date")]
    public DateTime? LastStampDate { get; set; }
}
