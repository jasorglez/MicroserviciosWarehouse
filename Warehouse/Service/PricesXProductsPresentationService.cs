using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse.Service;

public class PricesXProductsPresentationService : IPricesXProductsPresentationService
{
    private readonly DbWarehouseContext _context;        
    private readonly ILogger<PricesXProductsPresentationService> _logger;

    public PricesXProductsPresentationService(DbWarehouseContext dbContext, ILogger<PricesXProductsPresentationService> logger)
    {
        _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));;
        _logger = logger  ?? throw new ArgumentNullException(nameof(logger));
    }


    public async Task<List<object>> GetPrices()
    {
        try
        {
            return await _context.PricesXProductsPresentation
                .Select(w => new
                {
                    w.Id,
                    w.IdCatalogs,
                    w.IdMaterials,
                    w.IdMeasures,
                    w.Description,
                    w.Price,
                    w.Active
                })
                .AsNoTracking()
                .ToListAsync<object>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving prices by products");
            throw;
        }
    }

    public async Task<object> GetPricesById(int idPrice)
    {
        try
        {
            return await _context.PricesXProductsPresentation
                .Where(w => (w.Id == idPrice))
                .Select(w => new
                {
                    w.Id,
                    w.IdCatalogs,
                    w.IdMaterials,
                    w.IdMeasures,
                    w.Description,
                    w.Price,
                    w.Active
                })
                .AsNoTracking()
                .ToListAsync<object>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving prices by products");
            throw;
        }
    }

    public async Task Save(PricesXProductsPresentation wh)
    {
        try
        {
            _context.PricesXProductsPresentation.Add(wh);
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

    public async Task<bool> Update(int id, PricesXProductsPresentation pricesXProductsPresentation)
    {
        var existingWarehouse = await _context.PricesXProductsPresentation.FindAsync(id);
        if (existingWarehouse == null)
        {
            return false;
        }

        try
        {
            _context.Entry(existingWarehouse).CurrentValues.SetValues(pricesXProductsPresentation);
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

    public async Task<bool> Delete(int idPrice)
    {
        var existing = await _context.PricesXProductsPresentation.FindAsync(idPrice);
        if (existing == null)
        {
            _logger.LogWarning("Attempted to update non-existent Warehouses With ID {Id}", idPrice);
            return false;
        }

        try
        {
            existing.Active = false;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating Catalog", idPrice);
            throw;
        }
    }
}

public interface IPricesXProductsPresentationService
{
    Task<List<object>> GetPrices();
    Task<object> GetPricesById(int idPrice);
    Task Save(PricesXProductsPresentation wh);
    Task<bool> Update(int id, PricesXProductsPresentation pricesXProductsPresentation);
    Task<bool> Delete(int idPrice);
}