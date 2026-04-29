using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse.Service
{
    public class StampResult
    {
        public CustomerLoyaltyCard Card    { get; set; } = null!;
        public bool RewardEarned           { get; set; }
        public string RewardDescription    { get; set; } = "";
    }

    public interface ICustomerLoyaltyCardService
    {
        Task<List<CustomerLoyaltyCard>> GetByCustomerAsync(int idCustomer);
        Task<List<CustomerLoyaltyCard>> GetByProgramAsync(int idLoyaltyProgram);
        Task<CustomerLoyaltyCard?> GetByCustomerAndProgramAsync(int idCustomer, int idLoyaltyProgram);
        Task<CustomerLoyaltyCard> CreateAsync(CustomerLoyaltyCard card);
        Task<StampResult?> AddStampAsync(int idCustomer, int idLoyaltyProgram);
    }

    public class CustomerLoyaltyCardService : ICustomerLoyaltyCardService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<CustomerLoyaltyCardService> _logger;

        public CustomerLoyaltyCardService(DbWarehouseContext context, ILogger<CustomerLoyaltyCardService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        public async Task<List<CustomerLoyaltyCard>> GetByCustomerAsync(int idCustomer)
            => await _context.CustomerLoyaltyCards
                .Where(c => c.IdCustomer == idCustomer)
                .AsNoTracking()
                .ToListAsync();

        public async Task<List<CustomerLoyaltyCard>> GetByProgramAsync(int idLoyaltyProgram)
            => await _context.CustomerLoyaltyCards
                .Where(c => c.IdLoyaltyProgram == idLoyaltyProgram)
                .AsNoTracking()
                .ToListAsync();

        public async Task<CustomerLoyaltyCard?> GetByCustomerAndProgramAsync(int idCustomer, int idLoyaltyProgram)
            => await _context.CustomerLoyaltyCards
                .FirstOrDefaultAsync(c => c.IdCustomer == idCustomer && c.IdLoyaltyProgram == idLoyaltyProgram);

        public async Task<CustomerLoyaltyCard> CreateAsync(CustomerLoyaltyCard card)
        {
            card.CurrentStamps      = 0;
            card.TotalRewardsEarned = 0;
            card.LastStampDate      = null;
            _context.CustomerLoyaltyCards.Add(card);
            await _context.SaveChangesAsync();
            return card;
        }

        public async Task<StampResult?> AddStampAsync(int idCustomer, int idLoyaltyProgram)
        {
            var program = await _context.LoyaltyPrograms.FindAsync(idLoyaltyProgram);
            if (program == null) return null;

            var card = await _context.CustomerLoyaltyCards
                .FirstOrDefaultAsync(c => c.IdCustomer == idCustomer && c.IdLoyaltyProgram == idLoyaltyProgram);

            if (card == null)
            {
                card = new CustomerLoyaltyCard
                {
                    IdCustomer        = idCustomer,
                    IdLoyaltyProgram  = idLoyaltyProgram,
                    CurrentStamps     = 0,
                    TotalRewardsEarned = 0
                };
                _context.CustomerLoyaltyCards.Add(card);
            }

            card.CurrentStamps++;
            card.LastStampDate = DateTime.Now;

            bool rewardEarned = card.CurrentStamps >= program.StampsRequired;
            if (rewardEarned)
            {
                card.TotalRewardsEarned++;
                card.CurrentStamps = 0;
            }

            await _context.SaveChangesAsync();

            return new StampResult
            {
                Card              = card,
                RewardEarned      = rewardEarned,
                RewardDescription = rewardEarned ? program.RewardDescription : ""
            };
        }
    }
}
