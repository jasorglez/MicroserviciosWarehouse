using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models;

[Table("loyalty_programs")]
public class LoyaltyProgram
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_company")]
    public int IdCompany { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("id_product")]
    public int? IdProduct { get; set; }

    [Column("stamps_required")]
    public int StampsRequired { get; set; }

    [Column("reward_description")]
    public string? RewardDescription { get; set; }

    [Column("active")]
    public bool Active { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
}
