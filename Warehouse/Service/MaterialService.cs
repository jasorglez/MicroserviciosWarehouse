using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehouse.Models;


namespace Warehouse.Service
{
    public class MaterialService : IMaterialService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<MaterialService> _logger;

        public MaterialService(DbWarehouseContext context, ILogger<MaterialService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<object>> GetSupplies(int idCompany)
        {
            try
            {
                return await _context.Materials
                    .Where(s => s.Active && idCompany==idCompany)
                    .Select(s => new
                    {
                        s.Id,
                        s.IdCompany,
                        s.Insumo,
                        s.Articulo,
                        s.IdFamilia,
                        s.IdMedida,
                        s.IdUbication,
                        s.Description,
                        s.Date,
                        s.AplicaResg,
                        s.CostoMN,
                        s.CostoDLL,
                        s.VentaMN,
                        s.VentaDLL,
                        s.StockMin,
                        s.StockMax,
                        s.Picture
                    })
                    .AsNoTracking()
                    .ToListAsync<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Supplies");
                throw;
            }
        }

        public async Task Save(Material material)
        {
            try
            {
                _context.Materials.Add(material);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Supply");
                throw;
            }
        }

        public async Task<Material?> Update(int id, Material material)
        {
            var existingItem = await _context.Materials.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to update non-existent Supply with ID {Id}", id);
                return null;
            }

            try
            {
                _context.Entry(existingItem).CurrentValues.SetValues(material);
                await _context.SaveChangesAsync();
                return existingItem;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating Supply with ID {Id}", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Supply with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            var existingItem = await _context.Materials.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to delete non-existent Supply with ID {Id}", id);
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
                _logger.LogError(ex, "Error deleting Supply with ID {Id}", id);
                throw;
            }
        }
    }

    public interface IMaterialService
    {
        Task<List<object>> GetSupplies(int idCompany);
        Task Save(Material material);
        Task<Material?> Update(int id, Material material);
        Task<bool> Delete(int id);
    }

}