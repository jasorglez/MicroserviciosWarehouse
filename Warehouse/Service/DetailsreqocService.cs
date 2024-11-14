
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehouse.Models;

namespace Warehouse.Service
{
    public class DetailsreqocService : IDetailsreqocService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<DetailsreqocService> _logger;

        public DetailsreqocService(DbWarehouseContext context, ILogger<DetailsreqocService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<object>> GetDetails(int idMovement)
        {
            try
            {
                return await _context.Detailsreqoc
                    .Where(d => d.Active == true && d.IdMovement == idMovement)
                    .Select(d => new
                    {
                        d.Id,
                        d.IdMovement,
                        d.IdSupplie,
                        d.Quantity,
                        d.Price,
                        d.Total,
                        d.Type,
                        d.Comment
                    })
                    .AsNoTracking()
                    .ToListAsync<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Details for movement {IdMovement}", idMovement);
                throw;
            }
        }

        public async Task Save(Detailsreqoc detail)
        {
            try
            {
                _context.Detailsreqoc.Add(detail);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Detail");
                throw;
            }
        }

        public async Task<Detailsreqoc?> Update(int id, Detailsreqoc detail)
        {
            var existingItem = await _context.Detailsreqoc.FindAsync(id);
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
            var existingItem = await _context.Detailsreqoc.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to delete non-existent Detail with ID {Id}", id);
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
                _logger.LogError(ex, "Error deleting Detail with ID {Id}", id);
                throw;
            }
        }
    }

    public interface IDetailsreqocService
    {
        Task<List<object>> GetDetails(int idMovement);
        Task Save(Detailsreqoc detail);
        Task<Detailsreqoc?> Update(int id, Detailsreqoc detail);
        Task<bool> Delete(int id);
    }
}