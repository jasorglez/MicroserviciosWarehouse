using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse.Service
{
    public class PrefixSetupService : IPrefixSetupService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<PrefixSetupService> _logger;

        public PrefixSetupService(DbWarehouseContext context, ILogger<PrefixSetupService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PrefixSetup?> GetPrefixSetupById(int id)
        {
            try
            {
                return await _context.PrefixSetups
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ps => ps.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving prefix setup with ID {Id}.", id);
                throw;
            }
        }

        public async Task<PrefixSetup?> GetPrefixSetupByType(string type, int idProjectOrBranch)
        {
            try
            {
                return await _context.PrefixSetups
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ps => ps.Type == type && ps.IdProjectOrBranch == idProjectOrBranch && ps.Active);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving prefix setup with type {Type} for project/branch {IdProjectOrBranch}.", type, idProjectOrBranch);
                throw;
            }
        }

        public async Task Save(PrefixSetup prefixSetup)
        {
            try
            {
                _context.PrefixSetups.Add(prefixSetup);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving prefix setup.");
                throw;
            }
        }

        public async Task<bool> Update(int id, PrefixSetup prefixSetup)
        {
            var existingPrefixSetup = await _context.PrefixSetups
                .FirstOrDefaultAsync(ps => ps.Id == id);

            if (existingPrefixSetup == null)
            {
                return false;
            }

            try
            {
                existingPrefixSetup.IdProjectOrBranch = prefixSetup.IdProjectOrBranch;
                existingPrefixSetup.Type = prefixSetup.Type;
                existingPrefixSetup.PrefixReq = prefixSetup.PrefixReq;
                existingPrefixSetup.ConsecutiveReq = prefixSetup.ConsecutiveReq;
                existingPrefixSetup.PrefixCotiz = prefixSetup.PrefixCotiz;
                existingPrefixSetup.ConsecutiveCotiz = prefixSetup.ConsecutiveCotiz;
                existingPrefixSetup.PrefixOc = prefixSetup.PrefixOc;
                existingPrefixSetup.ConsecutiveOc = prefixSetup.ConsecutiveOc;
                existingPrefixSetup.Active = prefixSetup.Active;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating prefix setup with ID {Id}.", id);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            var existingPrefixSetup = await _context.PrefixSetups
                .FirstOrDefaultAsync(ps => ps.Id == id);

            if (existingPrefixSetup == null)
            {
                _logger.LogWarning("Attempted to delete non-existent prefix setup with ID {Id}", id);
                return false;
            }

            try
            {
                existingPrefixSetup.Active = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting prefix setup with ID {Id}.", id);
                throw;
            }
        }
    }

    public interface IPrefixSetupService
    {
        Task<PrefixSetup?> GetPrefixSetupById(int id);
        Task<PrefixSetup?> GetPrefixSetupByType(string type, int idProjectOrBranch);
        Task Save(PrefixSetup prefixSetup);
        Task<bool> Update(int id, PrefixSetup prefixSetup);
        Task<bool> Delete(int id);
    }
}
