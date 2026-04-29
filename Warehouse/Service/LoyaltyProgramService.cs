using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse.Service
{
    public interface ILoyaltyProgramService
    {
        Task<List<LoyaltyProgram>> GetAllAsync(int idCompany);
        Task<LoyaltyProgram?> GetByIdAsync(int id);
        Task<LoyaltyProgram> CreateAsync(LoyaltyProgram program);
        Task<LoyaltyProgram?> UpdateAsync(int id, LoyaltyProgram program);
        Task<bool> DeleteAsync(int id);
    }

    public class LoyaltyProgramService : ILoyaltyProgramService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<LoyaltyProgramService> _logger;

        public LoyaltyProgramService(DbWarehouseContext context, ILogger<LoyaltyProgramService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        public async Task<List<LoyaltyProgram>> GetAllAsync(int idCompany)
            => await _context.LoyaltyPrograms
                .Where(p => p.IdCompany == idCompany)
                .OrderBy(p => p.Name)
                .AsNoTracking()
                .ToListAsync();

        public async Task<LoyaltyProgram?> GetByIdAsync(int id)
            => await _context.LoyaltyPrograms.FindAsync(id);

        public async Task<LoyaltyProgram> CreateAsync(LoyaltyProgram program)
        {
            program.CreatedAt = DateTime.Now;
            _context.LoyaltyPrograms.Add(program);
            await _context.SaveChangesAsync();
            return program;
        }

        public async Task<LoyaltyProgram?> UpdateAsync(int id, LoyaltyProgram program)
        {
            var existing = await _context.LoyaltyPrograms.FindAsync(id);
            if (existing == null) return null;

            existing.Name               = program.Name;
            existing.IdProduct          = program.IdProduct;
            existing.StampsRequired     = program.StampsRequired;
            existing.RewardDescription  = program.RewardDescription;
            existing.Active             = program.Active;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _context.LoyaltyPrograms.FindAsync(id);
            if (existing == null) return false;
            _context.LoyaltyPrograms.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
