using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse.Service
{
    public class WarehouseService : IWarehouseService
    {
        private readonly DbWarehouseContext _context;        
        private readonly ILogger<WarehouseService> _logger;

        public WarehouseService(DbWarehouseContext dbContext, ILogger<WarehouseService> logger)
        {
            _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));            
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<object>> GetWarehouses(int idBusinnes)
        {
            try
            {
                return await _context.Warehouses
                    .Where(w => ( w.IdBussines == idBusinnes  && w.Active == true))
                    .Select(w => new
                    {
                        w.Id,
                        w.IdBussines,w.IdBranch,
                        w.Name,
                        w.Address,
                        w.City,w.CodePostal,
                        w.Place,
                        w.State,
                        w.Principal,                       
                        w.Phone,
                        w.Leader, w.Active
                    })
                    .AsNoTracking()
                    .ToListAsync<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving warehouses for company {IdBranch}", idBusinnes);
                throw;
            }
        }

        public async Task Save(Warehouset wh)
        {
            try
            {
                _context.Warehouses.Add(wh);
                await _context.SaveChangesAsync();                
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update error while saving warehouse");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving warehouse");
                throw;
            }
        }

        public async Task<bool> Update(int id, Warehouset warehousest)
        {
            var existingWarehouse = await _context.Warehouses.FindAsync(id);
            if (existingWarehouse == null)
            {
                return false;
            }

            try
            {
                _context.Entry(existingWarehouse).CurrentValues.SetValues(warehousest);
                await _context.SaveChangesAsync();                
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error while updating warehouse with ID {Id}", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating warehouse with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            var existing = await _context.Warehouses.FindAsync(id);
            if (existing == null)
            {
                _logger.LogWarning("Attempted to update non-existent Warehouses With ID {Id}", id);
                return false;
            }
            try
            {
                existing.Active = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error occurred while updating Catalogs", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating Catalog", id);
                throw;
            }
        }


    }

    public interface IWarehouseService
    {
        Task<List<object>> GetWarehouses(int idBusinnes);
        Task Save(Warehouset wh);
        Task<bool> Update(int id, Warehouset warehousest);
        Task<bool> Delete(int id);
    }

}
