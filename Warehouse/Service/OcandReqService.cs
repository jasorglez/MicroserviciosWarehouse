
using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse.Service
{
    public class OcandreqService : IOcandreqService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<OcandreqService> _logger;

        public OcandreqService(DbWarehouseContext context, ILogger<OcandreqService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<object>> GetOrders(string typeReference, int idReference, string type)
        {
            try
            {
                return await _context.Ocandreqs
                    .Where(o => o.Active == true && typeReference == o.TypeReference && idReference == o.IdReference && type == o.Type)
                    .Select(o => new
                    {
                        o.Id,
                        o.Folio,
                        o.TypeReference,
                        o.IdReference,
                        o.IdReq,
                        o.DateCreate,
                        o.IdDepartament,
                        o.Delivery,
                        o.DeliveryTime,
                        o.TypeOc,
                        o.DateSupply,
                        o.IdPayment,
                        o.IdCurrency,
                        o.Conditions,
                        o.IdAuthorize,
                        o.IdSolicit,
                        o.IdProvider,
                        o.Solicit,
                        o.Priority,
                        o.Type,
                        o.Comments,
                        o.IvaRetention,
                        o.Address,
                        o.City,
                        o.Phone,
                        o.Discount,
                        o.Active
                    })
                    .AsNoTracking()
                    .ToListAsync<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Orders");
                throw;
            }
        }

        public async Task<object?> GetOrderById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid Order ID provided: {Id}", id);
                    return null;
                }

                return await _context.Ocandreqs
                    .Where(o => o.Active == true && o.Id == id)
                    .Select(o => new
                    {
                        o.Id,
                        o.Folio,
                        o.TypeReference,
                        o.IdReference,
                        o.IdReq,
                        o.DateCreate,
                        o.IdDepartament,
                        o.Delivery,
                        o.DeliveryTime,
                        o.TypeOc,
                        o.DateSupply,
                        o.IdPayment,
                        o.IdCurrency,
                        o.Conditions,
                        o.IdAuthorize,
                        o.IdSolicit,
                        o.IdProvider,
                        o.Solicit,
                        o.Priority,
                        o.Type,
                        o.Comments,
                        o.IvaRetention,
                        o.Address,
                        o.City,
                        o.Phone,
                        o.Discount,
                        o.Active
                    })
            .AsNoTracking()
            .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Order by ID {Id}", id);
                throw;
            }
        }

        public async Task Save(Ocandreq ocandreq)
        {
            try
            {
                _context.Ocandreqs.Add(ocandreq);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Order");
                throw;
            }
        }

        public async Task<Ocandreq?> Update(int id, Ocandreq ocandreq)
        {
            var existingItem = await _context.Ocandreqs.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to update non-existent Order with ID {Id}", id);
                return null;
            }

            try
            {
                _context.Entry(existingItem).CurrentValues.SetValues(ocandreq);
                await _context.SaveChangesAsync();
                return existingItem;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating Order with ID {Id}", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Order with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            var existingItem = await _context.Ocandreqs.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to delete non-existent Order with ID {Id}", id);
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
                _logger.LogError(ex, "Error deleting Order with ID {Id}", id);
                throw;
            }
        }
    }

    public interface IOcandreqService
    {
        Task<List<object>> GetOrders(string TypeReference, int idReference, string type);
        Task<object?> GetOrderById(int id);
        Task Save(Ocandreq ocandreq);
        Task<Ocandreq?> Update(int id, Ocandreq ocandreq);
        Task<bool> Delete(int id);
    }
}