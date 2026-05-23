using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse.Service;

public interface ICustomerLoyaltyCardService
{
    Task<AddStampResult> AddStampAsync(int idCustomer, int idLoyaltyProgram);
}

public record AddStampResult(bool RewardEarned, string? RewardDescription, CardInfo Card);
public record CardInfo(int CurrentStamps);

public class CustomerLoyaltyCardService : ICustomerLoyaltyCardService
{
    private readonly DbWarehouseContext _context;
    private readonly ILogger<CustomerLoyaltyCardService> _logger;

    public CustomerLoyaltyCardService(DbWarehouseContext context, ILogger<CustomerLoyaltyCardService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AddStampResult> AddStampAsync(int idCustomer, int idLoyaltyProgram)
    {
        var program = await _context.LoyaltyPrograms
            .FirstOrDefaultAsync(lp => lp.Id == idLoyaltyProgram && lp.Active)
            ?? throw new InvalidOperationException($"Loyalty program {idLoyaltyProgram} not found or inactive");

        var card = await _context.CustomerLoyaltyCards
            .FirstOrDefaultAsync(c => c.IdCustomer == idCustomer && c.IdLoyaltyProgram == idLoyaltyProgram);

        if (card == null)
        {
            card = new CustomerLoyaltyCard
            {
                IdCustomer = idCustomer,
                IdLoyaltyProgram = idLoyaltyProgram,
                CurrentStamps = 0,
                TotalRewardsEarned = 0
            };
            _context.CustomerLoyaltyCards.Add(card);
        }

        card.CurrentStamps++;
        card.LastStampDate = DateTime.UtcNow;

        bool rewardEarned = card.CurrentStamps >= program.StampsRequired;
        if (rewardEarned)
        {
            card.CurrentStamps = 0;
            card.TotalRewardsEarned++;
        }

        await _context.SaveChangesAsync();

        return new AddStampResult(rewardEarned, program.RewardDescription, new CardInfo(card.CurrentStamps));
    }
}
