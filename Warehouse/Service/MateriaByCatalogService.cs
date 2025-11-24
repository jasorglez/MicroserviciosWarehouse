using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse.Service;

public class MateriaByCatalogService : IMateriaByCatalogService
{
    private readonly ILogger<MateriaByCatalogService> _logger;
    private readonly DbWarehouseContext _context;
    
    public MateriaByCatalogService(DbWarehouseContext context, ILogger<MateriaByCatalogService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task<List<object>> GetMateriaByCatalogsAsync(int idCompany, int idMaterial)
    {
        try
        {
            return await _context.MateriaByCatalog
                .Where(rm => rm.Active == true && rm.IdCompany == idCompany && rm.IdConcep == idMaterial)
                .OrderByDescending(rm => rm.FechaCambio)
                .ToListAsync<object>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving raw materials for company {IdCompany}", idCompany);
            throw;
        }
    }

    
    public async Task<MateriaByCatalog> CreateMateriaByCatalogAsync(MateriaByCatalog MateriaByCatalog)
    {
        try
        {
            _context.MateriaByCatalog.Add(MateriaByCatalog);
            await _context.SaveChangesAsync();
            return MateriaByCatalog;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating raw material");
            throw;
        }
    }
    
    public async Task<MateriaByCatalog?> UpdateMateriaByCatalogAsync(int id, MateriaByCatalog MateriaByCatalog)
    {
        // Load existing entity from DB
        var existingItem = await _context.MateriaByCatalog.FindAsync(id);

        // If not found, return null so controller can return 404
        if (existingItem == null)
        {
            _logger.LogWarning("Attempted to update non-existent raw material with ID {Id}", id);
            return null;
        }

        if (MateriaByCatalog == null)
        {
            _logger.LogWarning("Request body was null when updating raw material with ID {Id}", id);
            throw new ArgumentNullException(nameof(MateriaByCatalog));
        }

        try
        {
            // Copy incoming values onto the tracked entity. This avoids EF interpreting the incoming object
            // as a new entity and inserting it. Preserve the primary key.
            _context.Entry(existingItem).CurrentValues.SetValues(MateriaByCatalog);
            existingItem.Id = id;

            await _context.SaveChangesAsync();
            return existingItem;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating raw material with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteMateriaByCatalogAsync(int id)
    {
        var existingItem = await _context.MateriaByCatalog.FindAsync(id);
        if (existingItem == null)
        {
            _logger.LogWarning("Attempted to delete non-existent raw material with ID {Id}", id);
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
            _logger.LogError(ex, "Error deleting raw material with ID {Id}", id);
            throw;
        }
    }
}

public interface IMateriaByCatalogService
{
    Task<List<object>> GetMateriaByCatalogsAsync(int idCompany, int idMaterial);
    Task<MateriaByCatalog> CreateMateriaByCatalogAsync(MateriaByCatalog MateriaByCatalog);
    Task<MateriaByCatalog?> UpdateMateriaByCatalogAsync(int id, MateriaByCatalog MateriaByCatalog);
    Task<bool> DeleteMateriaByCatalogAsync(int id);
}