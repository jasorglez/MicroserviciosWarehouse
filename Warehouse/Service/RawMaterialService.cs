using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse.Service;

public class RawMaterialService : IRawMaterialService
{
    private readonly ILogger<RawMaterialService> _logger;
    private readonly DbWarehouseContext _context;
    
    public RawMaterialService(DbWarehouseContext context, ILogger<RawMaterialService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task<List<RawMaterial>> GetRawMaterialsAsync(int idCompany)
    {
        try
        {
            return await _context.RawMaterial
                .Where(rm => rm.Active && rm.IdCompany == idCompany)
                .OrderByDescending(rm => rm.FechaCambio)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving raw materials for company {IdCompany}", idCompany);
            throw;
        }
    }
    
    public async Task<List<RawMaterial>> GetRawMaterialByIdAsync(int id)
    {
        try
        {
            return await _context.RawMaterial
                .Where(rm => rm.Active && rm.Id == id)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving raw material with ID {Id}", id);
            throw;
        }
    }
    
    public async Task<RawMaterial> CreateRawMaterialAsync(RawMaterial rawMaterial)
    {
        if (rawMaterial == null)
        {
            throw new ArgumentNullException(nameof(rawMaterial));
        }

        try
        {
            _context.RawMaterial.Add(rawMaterial);
            await _context.SaveChangesAsync();
            return rawMaterial;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating raw material");
            throw;
        }
    }
    
    public async Task<RawMaterial> UpdateRawMaterialAsync(int id, RawMaterial rawMaterial)
    {
        var existingItem = await _context.RawMaterial.FindAsync(id);
        if (rawMaterial == null)
        {
            _logger.LogWarning("Attempted to update non-existent raw material with ID {Id}", id);
            throw new ArgumentNullException(nameof(rawMaterial));
        }

        try
        {
            _context.RawMaterial.Update(rawMaterial);
            await _context.SaveChangesAsync();
            return existingItem;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating raw material with ID {Id}", rawMaterial.Id);
            throw;
        }
    }

    public async Task<bool> DeleteRawMaterialAsync(int id)
    {
        var existingItem = await _context.RawMaterial.FindAsync(id);
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

public interface IRawMaterialService
{
    Task<List<RawMaterial>> GetRawMaterialsAsync(int idCompany);
    Task<List<RawMaterial>> GetRawMaterialByIdAsync(int id);
    Task<RawMaterial> CreateRawMaterialAsync(RawMaterial rawMaterial);
    Task<RawMaterial> UpdateRawMaterialAsync(int id, RawMaterial rawMaterial);
    Task<bool> DeleteRawMaterialAsync(int id);
}