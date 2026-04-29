using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse.Service
{
    public interface ILoyaltyProgramProductService
    {
        Task<List<LoyaltyProgram>> GetProgramsByProductAsync(int idCompany, int idProduct);
        Task<List<Material>> GetProductsByProgramAsync(int idLoyaltyProgram);
        Task AddProductAsync(int idLoyaltyProgram, int idProduct);
        Task RemoveProductAsync(int idLoyaltyProgram, int idProduct);
        Task ClearProductsAsync(int idLoyaltyProgram);
    }

    public class LoyaltyProgramProductService : ILoyaltyProgramProductService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<LoyaltyProgramProductService> _logger;

        public LoyaltyProgramProductService(DbWarehouseContext context, ILogger<LoyaltyProgramProductService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        public async Task<List<LoyaltyProgram>> GetProgramsByProductAsync(int idCompany, int idProduct)
            => await _context.LoyaltyProgramProducts
                .Where(lpp => lpp.IdProduct == idProduct)
                .Join(_context.LoyaltyPrograms,
                    lpp => lpp.IdLoyaltyProgram,
                    lp => lp.Id,
                    (lpp, lp) => lp)
                .Where(lp => lp.IdCompany == idCompany && lp.Active)
                .AsNoTracking()
                .ToListAsync();

        public async Task<List<Material>> GetProductsByProgramAsync(int idLoyaltyProgram)
            => await _context.LoyaltyProgramProducts
                .Where(lpp => lpp.IdLoyaltyProgram == idLoyaltyProgram)
                .Join(_context.Materials,
                    lpp => lpp.IdProduct,
                    m => m.Id,
                    (lpp, m) => m)
                .AsNoTracking()
                .ToListAsync();

        public async Task AddProductAsync(int idLoyaltyProgram, int idProduct)
        {
            var exists = await _context.LoyaltyProgramProducts
                .FirstOrDefaultAsync(lpp => lpp.IdLoyaltyProgram == idLoyaltyProgram && lpp.IdProduct == idProduct);

            if (exists != null) return;

            _context.LoyaltyProgramProducts.Add(new LoyaltyProgramProduct
            {
                IdLoyaltyProgram = idLoyaltyProgram,
                IdProduct = idProduct
            });
            await _context.SaveChangesAsync();
        }

        public async Task RemoveProductAsync(int idLoyaltyProgram, int idProduct)
        {
            var item = await _context.LoyaltyProgramProducts
                .FirstOrDefaultAsync(lpp => lpp.IdLoyaltyProgram == idLoyaltyProgram && lpp.IdProduct == idProduct);

            if (item == null) return;

            _context.LoyaltyProgramProducts.Remove(item);
            await _context.SaveChangesAsync();
        }

        public async Task ClearProductsAsync(int idLoyaltyProgram)
        {
            var items = await _context.LoyaltyProgramProducts
                .Where(lpp => lpp.IdLoyaltyProgram == idLoyaltyProgram)
                .ToListAsync();

            _context.LoyaltyProgramProducts.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
    }
}
