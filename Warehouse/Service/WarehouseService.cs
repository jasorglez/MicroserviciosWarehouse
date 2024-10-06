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

        public async Task<List<object>> GetWarehouses(string idCompany, string idProject)
        {
            try
            {
                return await _context.Warehousest
                    .Where(w => (w.IdCompany == idCompany && w.IdProject == idProject) || w.Principal == "SI")
                    .Select(w => new
                    {
                        w.Id,
                        w.Name,
                        w.Address,
                        w.City,
                        w.Place,
                        w.State,
                        w.Principal,
                        w.Cp,
                        w.Phone,
                        w.Leader
                    })
                    .AsNoTracking()
                    .ToListAsync<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving warehouses for company {IdCompany} and project {IdProject}", idCompany, idProject);
                throw;
            }
        }

        public async Task Save(Warehouset wh)
        {
            try
            {
                _context.Warehousest.Add(wh);
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
            var existingWarehouse = await _context.Warehousest.FindAsync(id);
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


    }


    public interface IWarehouseService
    {
        Task<List<object>> GetWarehouses(string idCompany, string idProject);
        Task Save(Warehouset wh);
        Task<bool> Update(int id, Warehouset warehousest);
    }

}
