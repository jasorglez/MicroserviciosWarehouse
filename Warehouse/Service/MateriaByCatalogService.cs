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
            return await (
                from m in _context.MateriaByCatalog
                join mat in _context.Materials on m.IdCatalog equals mat.Id into matJoin
                from mat in matJoin.DefaultIfEmpty()
                where m.Active == true && m.IdCompany == idCompany && m.IdConcep == idMaterial
                orderby m.FechaCambio descending
                select (object)new
                {
                    m.Id,
                    m.IdCompany,
                    m.Check,
                    m.IdConcep,
                    m.IdCatalog,
                    articulo = mat != null ? mat.Description : null,
                    m.CostoUni,
                    m.Cantidad,
                    m.Proporcion,
                    m.CostoTot,
                    m.Merma,
                    m.CostoFin,
                    m.FechaCambio,
                    m.Parametros,
                    m.Total,
                    m.Active
                }
            ).ToListAsync();
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
            await ActualizarPorcentaje(MateriaByCatalog.IdCompany, MateriaByCatalog.IdConcep);
            await ActualizarCostoMaterial(MateriaByCatalog.IdCompany, MateriaByCatalog.IdConcep);
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
            await ActualizarPorcentaje(existingItem.IdCompany, existingItem.IdConcep);
            await ActualizarCostoMaterial(existingItem.IdCompany, existingItem.IdConcep);
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
            var idCompany = existingItem.IdCompany;
            var idConcep = existingItem.IdConcep;
            
            existingItem.Active = false;
            await _context.SaveChangesAsync();
            
            await ActualizarPorcentaje(idCompany, idConcep);
            await ActualizarCostoMaterial(idCompany, idConcep);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting raw material with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> ActualizarPorcentaje(int? IdCompany,int? IdConcep)
    {
        var listaActiva = await _context.MateriaByCatalog
            .Where(rm => rm.Active == true && rm.IdCompany == IdCompany && rm.IdConcep == IdConcep && rm.Check == true)
            .ToListAsync();
        var cantidad = listaActiva.Sum(x => x.Cantidad) ?? 0;

        foreach (var item in listaActiva)
        {
            if (cantidad > 0)
            {
                item.Proporcion = (item.Cantidad / cantidad) * 100;
            }
            else
            {
                item.Proporcion = 0;
            }
            _context.MateriaByCatalog.Update(item);
        }
        var listaInactiva = await _context.MateriaByCatalog
            .Where(rm => rm.Active == true && rm.IdCompany == IdCompany && rm.IdConcep == IdConcep && rm.Check == false)
            .ToListAsync();
        foreach (var item in listaInactiva)
        {
            item.Proporcion = 0;
            _context.MateriaByCatalog.Update(item);
        }
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActualizarCostoMaterial(int? IdCompany, int? IdConcep)
    {
        try
        {
            // Buscar el material por IdConcep (que es el id del material)
            var material = await _context.Materials.FirstOrDefaultAsync(m => m.Id == IdConcep);
            if (material == null)
            {
                _logger.LogWarning("Material no encontrado con Id {Id}", IdConcep);
                return false;
            }

            // Calcular la suma de CostoTot de todos los MateriaByCatalog activos para este material
            var costoTotal = await _context.MateriaByCatalog
                .Where(mbc => mbc.Active == true && mbc.IdConcep == IdConcep && mbc.Check == true)
                .SumAsync(mbc => mbc.CostoTot ?? 0);

            // Actualizar el campo Costo del material
            material.Costo = costoTotal;
            _context.Materials.Update(material);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Costo actualizado para material {Id}: {Costo}", IdConcep, costoTotal);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando costo del material {Id}", IdConcep);
            return false;
        }
    }
}

public interface IMateriaByCatalogService
{
    Task<List<object>> GetMateriaByCatalogsAsync(int idCompany, int idMaterial);
    Task<MateriaByCatalog> CreateMateriaByCatalogAsync(MateriaByCatalog MateriaByCatalog);
    Task<MateriaByCatalog?> UpdateMateriaByCatalogAsync(int id, MateriaByCatalog MateriaByCatalog);
    Task<bool> DeleteMateriaByCatalogAsync(int id);
    Task<bool> ActualizarPorcentaje(int? IdCompany, int? IdConcep);
    Task<bool> ActualizarCostoMaterial(int? IdCompany, int? IdConcep);
}