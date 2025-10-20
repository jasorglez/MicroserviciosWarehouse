
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehouse.Models;
using Warehouse.Models.DTOs;

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
                    .Where(d => d.Active == true && idMovement == d.IdMovement)
                    .Join(_context.Materials,
                        d => d.IdSupplie,
                        m => m.Id,
                        (d, m) => new { Details = d, Material = m })
                    .Join(_context.Catalogs,
                        dm => dm.Material.IdMedida,
                        c => c.Id,
                        (dm, c) => new
                        {
                            dm.Details.Id,
                            dm.Details.IdMovement,
                            dm.Details.IdSupplie,
                            code = dm.Material.Insumo,
                            description = dm.Material.Description,
                            measure = c.Description,  // Descripción de la unidad de medida
                            dm.Details.Quantity,
                            dm.Details.Price,
                            dm.Details.Total,
                            dm.Details.Type,
                            dm.Details.Comment,
                            dm.Details.Dateuse,
                            dm.Details.Active
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

        // Query Syntax (Alternative)
        public async Task<List<PurchaseOrderDetail>> GetPurchaseOrderDetailsQuery(int idProv)
        {

            var result = await (
                from o in _context.Ocandreqs
                join d in _context.Detailsreqoc on o.Id equals d.IdMovement
                join m in _context.Materials on d.IdSupplie equals m.Id
                where o.Type == "OC" && o.IdProvider==idProv && d.Active == true 
                orderby o.Id
                select new PurchaseOrderDetail
                {
                    Id           = o.Id,
                    Folio        = o.Folio,
                    DateCreate   = o.DateCreate,
                    IdSupplie    = d.IdSupplie,
                    Description  = m.Description,
                    Quantity     = d.Quantity,
                    Price        = d.Price,
                    IdProvider   = o.IdProvider 
                })
                .AsNoTracking()
                .ToListAsync();

            return result;
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
        Task<List<PurchaseOrderDetail>> GetPurchaseOrderDetailsQuery(int idProv);
        Task Save(Detailsreqoc detail);
        Task<Detailsreqoc?> Update(int id, Detailsreqoc detail);
        Task<bool> Delete(int id);
    }
}