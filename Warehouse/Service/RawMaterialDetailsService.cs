using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse.Service;

public class RawMaterialDetailsService : IRawMaterialDetailsService
{
    private readonly ILogger<RawMaterialDetailsService> _logger;
    private readonly DbWarehouseContext _context;

    public RawMaterialDetailsService(DbWarehouseContext context, ILogger<RawMaterialDetailsService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task<List<RawMaterialDetails>> GetRawMaterialDetailsAsync(int idRawMaterial)
    {
        try
        {
            return await _context.RawMaterialDetails
                .Where(rmd => rmd.IdRawMaterial == idRawMaterial)
                .OrderByDescending(rmd => rmd.Id)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving raw material details for raw material ID {IdRawMaterial}", idRawMaterial);
            throw;
        }
    }
    
    public async Task<RawMaterialDetails> CreateRawMaterialDetailsAsync(RawMaterialDetails rawMaterialDetails)
    {
        if (rawMaterialDetails == null)
        {
            throw new ArgumentNullException(nameof(rawMaterialDetails));
        }

        try
        {
            _context.RawMaterialDetails.Add(rawMaterialDetails);
            await _context.SaveChangesAsync();
            return rawMaterialDetails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating raw material details");
            throw;
        }
    }
    
    public async Task<RawMaterialDetails> UpdateRawMaterialDetailsAsync(int id, RawMaterialDetails rawMaterialDetails)
    {
        var existingItem = await _context.RawMaterialDetails.FindAsync(id);
        if (rawMaterialDetails == null)
        {
            _logger.LogWarning("Raw material details with ID {Id} not found for update", id);
            throw new ArgumentNullException(nameof(rawMaterialDetails));
        }

        try
        {
            _context.RawMaterialDetails.Update(rawMaterialDetails);
            await _context.SaveChangesAsync();
            return rawMaterialDetails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating raw material details");
            throw;
        }
    }
    
    public async Task<bool> DeleteRawMaterialDetailsAsync(int id)
    {
        var existingItem = await _context.RawMaterialDetails.FindAsync(id);
        if (existingItem == null)
        {
            _logger.LogWarning("Attempted to delete non-existent raw material details with ID {Id}", id);
            return false;
        }

        try
        {
            _context.RawMaterialDetails.Remove(existingItem);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting raw material details with ID {Id}", id);
            throw;
        }
    }
    
    public async Task<RawMaterialDetails?> GetRawMaterialDetailsByIdAsync(int id)
    {
        try
        {
            return await _context.RawMaterialDetails.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving raw material details with ID {Id}", id);
            throw;
        }
    }
}

public interface IRawMaterialDetailsService
{
    Task<List<RawMaterialDetails>> GetRawMaterialDetailsAsync(int idRawMaterial);
    Task<RawMaterialDetails> CreateRawMaterialDetailsAsync(RawMaterialDetails rawMaterialDetails);
    Task<RawMaterialDetails> UpdateRawMaterialDetailsAsync(int id, RawMaterialDetails rawMaterialDetails);
    Task<bool> DeleteRawMaterialDetailsAsync(int id);
    Task<RawMaterialDetails?> GetRawMaterialDetailsByIdAsync(int id);
}