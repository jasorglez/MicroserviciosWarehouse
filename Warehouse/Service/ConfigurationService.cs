
using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse.Service
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<ConfigurationService> _logger;

        public ConfigurationService(DbWarehouseContext context, ILogger<ConfigurationService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<Configuration>> GetAllConfigurations(int idRoot)
        {
            try
            {
                return await _context.Configurations
                    .Where(c => (c.IdRoot== idRoot && c.Active==true))
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving configurations.");
                throw;
            }
        }

        public async Task<Configuration?> GetConfigurationById(int id)
        {
            try
            {
                return await _context.Configurations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving configuration with ID {Id}.", id);
                throw;
            }
        }

        public async Task Save(Configuration config)
        {
            try
            {
                _context.Configurations.Add(config);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving configuration.");
                throw;
            }
        }

        public async Task<bool> Update(int id, Configuration config)
        {
            var existingConfig = await _context.Configurations.FindAsync(id);
            if (existingConfig == null)
            {
                return false;
            }

            try
            {
                _context.Entry(existingConfig).CurrentValues.SetValues(config);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating configuration with ID {Id}.", id);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            var existingConfig = await _context.Configurations.FindAsync(id);
            if (existingConfig == null)
            {
                _logger.LogWarning("Attempted to update non-existent Configuration Warehouse With ID {Id}", id);
                return false;
            }

            try
            {
                existingConfig.Active = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting configuration with ID {Id}.", id);
                throw;
            }
        }
    }

    public interface IConfigurationService
    {
        Task<List<Configuration>> GetAllConfigurations(int idRoot);
        Task<Configuration?> GetConfigurationById(int id);
        Task Save(Configuration config);
        Task<bool> Update(int id, Configuration config);
        Task<bool> Delete(int id);
    }
}

