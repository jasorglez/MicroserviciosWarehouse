using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse.Service;

public class ParameterByMaterialDescriptionService : IParameterByMaterialDescriptionService
{
    private readonly ILogger<ParameterByMaterialDescriptionService> _logger;
    private readonly DbWarehouseContext _context;
    
    public ParameterByMaterialDescriptionService(DbWarehouseContext context, ILogger<ParameterByMaterialDescriptionService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /*public async Task<List<object>> GetParameterByMaterialDescriptionsAsync(int idCompany, int idMaterial)
    {
        try
        {
            return await _context.ParameterByMaterialDescription
                .Where(p => p.Active == true && p.Vigente == true)
                .ToListAsync<object>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parameters for company {IdCompany}", idCompany);
            throw;
        }
    }*/

    public async Task<List<object>> Getparameter(int idCompany)
    {
        try
        {
            return await _context.Catalogs
                .Where(c => c.Active == 1 && c.IdCompany == idCompany && c.Type == "PARAMETER")
                .OrderBy(c => c.Description)
                .ToListAsync<object>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving raw materials for company {IdCompany}", idCompany);
            throw;
        }
    }

    
    public async Task<ParameterByMaterialDescription> CreateParameterByMaterialDescriptionAsync(ParameterByMaterialDescription parameterByMaterialDescription)
    {
        try
        {
            _context.ParameterByMaterialDescription.Add(parameterByMaterialDescription);
            await _context.SaveChangesAsync();
            return parameterByMaterialDescription;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating parameter");
            throw;
        }
    }
    
    /*public async Task<ParameterByMaterialDescription?> UpdateParameterByMaterialDescriptionAsync(int id, ParameterByMaterialDescription parameterByMaterialDescription)
    {
        // Load existing entity from DB
        var existingItem = await _context.ParameterByMaterialDescription.FindAsync(id);

        // If not found, return null so controller can return 404
        if (existingItem == null)
        {
            _logger.LogWarning("Attempted to update non-existent raw material with ID {Id}", id);
            return null;
        }

        if (parameterByMaterialDescription == null)
        {
            _logger.LogWarning("Request body was null when updating parameter with ID {Id}", id);
            throw new ArgumentNullException(nameof(parameterByMaterialDescription));
        }

        try
        {
            // Copy incoming values onto the tracked entity. This avoids EF interpreting the incoming object
            // as a new entity and inserting it. Preserve the primary key.
            _context.Entry(existingItem).CurrentValues.SetValues(parameterByMaterialDescription);
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

    public async Task<bool> DeleteParameterByMaterialDescriptionAsync(int id)
    {
        var existingItem = await _context.ParameterByMaterialDescription.FindAsync(id);
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
    }*/

}

public interface IParameterByMaterialDescriptionService
{
    //Task<List<object>> GetParameterByMaterialDescriptionsAsync(int idCompany, int idMaterial);
    Task<List<object>> Getparameter(int idCompany);
    Task<ParameterByMaterialDescription> CreateParameterByMaterialDescriptionAsync(ParameterByMaterialDescription parameterByMaterialDescription);
    //Task<ParameterByMaterialDescription?> UpdateParameterByMaterialDescriptionAsync(int id, ParameterByMaterialDescription parameterByMaterialDescription);
    //Task<bool> DeleteParameterByMaterialDescriptionAsync(int id);
}