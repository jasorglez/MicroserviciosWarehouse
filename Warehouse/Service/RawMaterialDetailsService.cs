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
    
    public async Task<List<object>> GetRawMaterialDetailsAsync(int idRawMaterial)
    {
        try
        {
            return await _context.RawMaterialDetails
                .Where(rmd => rmd.IdRawMaterial == idRawMaterial)
                .Join(
                    _context.Materials,
                    rmd => rmd.IdMaterial,
                    m => m.Id,
                    (rmd, m) => new { rmd, MaterialDetalle = m })
                .Join(
                    _context.RawMaterial,
                    x => x.rmd.IdRawMaterial,
                    rm => rm.Id,
                    (x, rm) => new { x.rmd, x.MaterialDetalle, rm })
                .Join(
                    _context.Materials,
                    x => x.rm.IdMaterial,
                    m => m.Id,
                    (x, m) => new
                    {
                        x.rmd.Id,
                        x.rmd.IdRawMaterial,
                        x.rmd.IdMaterial,
                        ArticuloDetalle = x.MaterialDetalle.Description,
                        ArticuloMateriaPrima = m.Description,
                        x.rmd.Cost,
                        x.rmd.Quantity,
                        x.rmd.TotalCost
                    })
                .OrderByDescending(x => x.Id)
                .ToListAsync<object>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving raw material details for raw material ID {IdRawMaterial}", idRawMaterial);
            throw;
        }
    }
    
    public async Task<object?> GetRawMaterialDetailsByIdAsync(int id)
    {
        try
        {
            return await _context.RawMaterialDetails
                .Where(rmd => rmd.Id == id)
                .Join(
                    _context.Materials,
                    rmd => rmd.IdMaterial,
                    m => m.Id,
                    (rmd, m) => new { rmd, MaterialDetalle = m })
                .Join(
                    _context.RawMaterial,
                    x => x.rmd.IdRawMaterial,
                    rm => rm.Id,
                    (x, rm) => new { x.rmd, x.MaterialDetalle, rm })
                .Join(
                    _context.Materials,
                    x => x.rm.IdMaterial,
                    m => m.Id,
                    (x, m) => new
                    {
                        x.rmd.Id,
                        x.rmd.IdRawMaterial,
                        x.rmd.IdMaterial,
                        ArticuloDetalle = x.MaterialDetalle.Description,
                        ArticuloMateriaPrima = m.Description,
                        x.rmd.Cost,
                        x.rmd.Quantity,
                        x.rmd.TotalCost
                    })
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving raw material details with ID {Id}", id);
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
}

public interface IRawMaterialDetailsService
{
    Task<List<object>> GetRawMaterialDetailsAsync(int idRawMaterial);
    Task<object?> GetRawMaterialDetailsByIdAsync(int id);
    Task<RawMaterialDetails> CreateRawMaterialDetailsAsync(RawMaterialDetails rawMaterialDetails);
    Task<RawMaterialDetails> UpdateRawMaterialDetailsAsync(int id, RawMaterialDetails rawMaterialDetails);
    Task<bool> DeleteRawMaterialDetailsAsync(int id);
}