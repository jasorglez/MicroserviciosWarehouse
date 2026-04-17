using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse.Service;

    public class DetailsinandoutService : IDetailsinandoutService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<DetailsinandoutService> _logger;

        public DetailsinandoutService(DbWarehouseContext context, ILogger<DetailsinandoutService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<object>> GetDetails(int idInandout)
        {
            try
            {
                return await _context.Detailsinandout
                    .Where(d => d.Active == true && d.IdInandout == idInandout)
                    .Join(_context.Materials,
                        detail => detail.IdProduct,
                        material => material.Id,
                        (detail, material) => new { detail, material })
                    .Join(_context.Catalogs,
                        detailMaterial => detailMaterial.material.IdMedida,
                        catalog => catalog.Id,
                        (detailMaterial, catalog) => new
                        {
                            detailMaterial.detail.Id,
                            code = detailMaterial.material.Insumo,
                            detailMaterial.detail.IdInandout,
                            detailMaterial.detail.IdProduct,
                            description = detailMaterial.material.Description,
                            measure = catalog.Description,
                            detailMaterial.detail.Quantity,
                            detailMaterial.detail.Pending,
                            detailMaterial.detail.Total,
                            detailMaterial.detail.Active
                        })
                    .ToListAsync<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

    public async Task<object> GetTotalInByProductAndWarehouse(int productId, int warehouseId)
    {
        try
        {
            var total = await _context.Detailsinandout
                .Where(d => d.Active == true && d.IdProduct == productId)
                .Join(_context.Inandouts,
                    d => d.IdInandout,
                    i => i.Id,
                    (d, i) => new { d, i })
                .Join(_context.Warehouses,
                    di => di.i.IdWarehouse,
                    w => w.Id,
                    (di, w) => new { di.d, di.i, w })
                .Where(x => x.i.Type == "IN" && x.w.Id == warehouseId)
                .SumAsync(x => (decimal?)x.d.Quantity) ?? 0;

            return total;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving total IN quantity for product {ProductId} at warehouse {WarehouseId}", productId, warehouseId);
            throw;
        }
    }


    public async Task Save(Detailsinandout detail)
        {
            try
            {
                _context.Detailsinandout.Add(detail);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        
        public async Task<Detailsinandout?> Update(int id, Detailsinandout detail)
        {
            var existingItem = await _context.Detailsinandout.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to update non-existent Detail with ID {Id}", id);
                return null;
            }

            try
            {
                _context.Entry(existingItem).CurrentValues.SetValues(detail);
                await _context.SaveChangesAsync();
                return existingItem;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating Detail with ID {Id}", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Detail with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            var existingItem = await _context.Detailsinandout.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to delete non-existent detail with ID {Id}", id);
                return false;
            }

            try
            {
                existingItem.Active = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }

    public interface IDetailsinandoutService
    {
        Task<List<object>> GetDetails(int idInandout);
        Task<object> GetTotalInByProductAndWarehouse(int productId, int warehouseId);
        Task Save(Detailsinandout detail);
        Task<Detailsinandout?> Update(int id, Detailsinandout detail);
        Task<bool> Delete(int id);
    }
