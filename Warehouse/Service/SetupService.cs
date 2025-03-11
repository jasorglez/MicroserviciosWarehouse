
using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse.Service
{
    public class SetupService : ISetupService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<SetupService> _logger;

        public SetupService(DbWarehouseContext context, ILogger<SetupService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<Setup>> GetAllSetups(int idCompany)
        {
            try
            {
                return await _context.Setups
                    .Where(s => s.IdCompany == idCompany && s.Active)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving setups.");
                throw;
            }
        }

        public async Task<Setup?> GetSetupById(int id)
        {
            try
            {
                return await _context.Setups
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving setup with ID {Id}.", id);
                throw;
            }
        }

        public async Task Save(Setup setup)
        {
            try
            {
                _context.Setups.Add(setup);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving setup.");
                throw;
            }
        }

        public async Task<bool> Update(int id, Setup setup)
        {
            var existingSetup = await _context.Setups.FindAsync(id);
            if (existingSetup == null)
            {
                return false;
            }

            try
            {
                _context.Entry(existingSetup).CurrentValues.SetValues(setup);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating setup with ID {Id}.", id);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            var existingSetup = await _context.Setups.FindAsync(id);
            if (existingSetup == null)
            {
                _logger.LogWarning("Attempted to delete non-existent setup with ID {Id}", id);
                return false;
            }

            try
            {
                existingSetup.Active = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting setup with ID {Id}.", id);
                throw;
            }
        }
    }

    public interface ISetupService
    {
        Task<List<Setup>> GetAllSetups(int idCompany);
        Task<Setup?> GetSetupById(int id);
        Task Save(Setup setup);
        Task<bool> Update(int id, Setup setup);
        Task<bool> Delete(int id);
    }
}