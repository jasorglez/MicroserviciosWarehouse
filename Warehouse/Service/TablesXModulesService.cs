
using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse.Service
{
    public class TablesXModulesService : ITablesXModulesService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<TablesXModulesService> _logger;

        public TablesXModulesService(DbWarehouseContext context, ILogger<TablesXModulesService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Get all active tablesxmodules
        public async Task<List<TablesXModules>> GetAllTablesXModules(string table, string selectedSection)
        {
            try
            {
                return await _context.TablesXModules
                    .Where(t => t.Table == table && t.Active && t.Sections == selectedSection)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tablesxmodules.");
                throw;
            }
        }

         public async Task<List<TablesXModules>> GetTablesXModulesSections(string table)
            {
                try
                {
                    return await _context.TablesXModules
                        .Where(t => t.Table == table && t.Active)
                        .GroupBy(t => t.Sections)
                        .Select(g => g.First()) // o .FirstOrDefault()
                        .AsNoTracking()
                        .ToListAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving tablesxmodules.");
                    throw;
                }
            }


        // Get a specific tablesxmodules record by ID
        public async Task<TablesXModules?> GetTablesXModulesById(int id)
        {
            try
            {
                return await _context.TablesXModules
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tablesxmodules with ID {Id}.", id);
                throw;
            }
        }

        // Save a new tablesxmodules record
        public async Task Save(TablesXModules tablesXModules)
        {
            try
            {
                _context.TablesXModules.Add(tablesXModules);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving tablesxmodules.");
                throw;
            }
        }

        // Update an existing tablesxmodules record
        public async Task<bool> Update(int id, TablesXModules tablesXModules)
        {
            var existingRecord = await _context.TablesXModules.FindAsync(id);
            if (existingRecord == null)
            {
                return false;
            }

            try
            {
                // Update properties
                existingRecord.Name = tablesXModules.Name;
                existingRecord.NameSpanish = tablesXModules.NameSpanish;
                existingRecord.Table = tablesXModules.Table;
                existingRecord.Sections = tablesXModules.Sections;
                existingRecord.Active = tablesXModules.Active;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tablesxmodules with ID {Id}.", id);
                throw;
            }
        }

        // Soft delete a tablesxmodules record (set Active to false)
        public async Task<bool> Delete(int id)
        {
            var existingRecord = await _context.TablesXModules.FindAsync(id);
            if (existingRecord == null)
            {
                _logger.LogWarning("Attempted to delete non-existent tablesxmodules with ID {Id}", id);
                return false;
            }

            try
            {
                existingRecord.Active = false; // Soft delete
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tablesxmodules with ID {Id}.", id);
                throw;
            }
        }
    }

    // Interface for the service
    public interface ITablesXModulesService
    {
        Task<List<TablesXModules>> GetAllTablesXModules(string table, string selectedSection);
        Task<List<TablesXModules>> GetTablesXModulesSections(string table);
        Task<TablesXModules?> GetTablesXModulesById(int id);
        Task Save(TablesXModules tablesXModules);
        Task<bool> Update(int id, TablesXModules tablesXModules);
        Task<bool> Delete(int id);
    }
}