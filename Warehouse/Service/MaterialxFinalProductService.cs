using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehouse.Models;

namespace Warehouse.Service;

public class MaterialxFinalProductService : IMaterialxFinalProductService
{
    private readonly DbWarehouseContext _context;
    private readonly ILogger<MaterialxFinalProductService> _logger;

    public MaterialxFinalProductService(DbWarehouseContext context, ILogger<MaterialxFinalProductService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> ExistsByMaterialAndPresentation(int idMaterial, int idPresentation)
    {
        try
        {
            var exists = await _context.MaterialxFinalProducts
                .AnyAsync(m => m.idMaterial == idMaterial && m.idPresentation == idPresentation && m.Active == true);

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking MaterialxFinalProduct for IdMaterial {IdMaterial} and IdPresentation {IdPresentation}", idMaterial, idPresentation);
            throw;
        }
    }

    public async Task<bool> AddIfNotExists(int idMaterial, int idPresentation)
    {
        try
        {
            // Verificar si ya existe
            var exists = await _context.MaterialxFinalProducts
                .AnyAsync(m => m.idMaterial == idMaterial && m.idPresentation == idPresentation && m.Active == true);

            if (exists)
            {
                _logger.LogInformation("MaterialxFinalProduct already exists for IdMaterial {IdMaterial} and IdPresentation {IdPresentation}", idMaterial, idPresentation);
                return false; // No se creó porque ya existe
            }

            // Crear el nuevo registro
            var newRecord = new MaterialxFinalProduct
            {
                idMaterial = idMaterial,
                idPresentation = idPresentation,
                Active = true
            };

            _context.MaterialxFinalProducts.Add(newRecord);
            await _context.SaveChangesAsync();

            _logger.LogInformation("MaterialxFinalProduct created for IdMaterial {IdMaterial} and IdPresentation {IdPresentation}", idMaterial, idPresentation);
            return true; // Se creó exitosamente
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding MaterialxFinalProduct for IdMaterial {IdMaterial} and IdPresentation {IdPresentation}", idMaterial, idPresentation);
            throw;
        }
    }

    public async Task<bool> Delete(int idMaterial, int idPresentation)
    {
        try
        {
            var existingItem = await _context.MaterialxFinalProducts
                .FirstOrDefaultAsync(m => m.idMaterial == idMaterial && m.idPresentation == idPresentation && m.Active == true);

            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to delete non-existent MaterialxFinalProduct with IdMaterial {IdMaterial} and IdPresentation {IdPresentation}", idMaterial, idPresentation);
                return false;
            }

            existingItem.Active = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("MaterialxFinalProduct soft deleted for IdMaterial {IdMaterial} and IdPresentation {IdPresentation}", idMaterial, idPresentation);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting MaterialxFinalProduct for IdMaterial {IdMaterial} and IdPresentation {IdPresentation}", idMaterial, idPresentation);
            throw;
        }
    }
}

public interface IMaterialxFinalProductService
{
    Task<bool> ExistsByMaterialAndPresentation(int idMaterial, int idPresentation);
    Task<bool> AddIfNotExists(int idMaterial, int idPresentation);
    Task<bool> Delete(int idMaterial, int idPresentation);
}