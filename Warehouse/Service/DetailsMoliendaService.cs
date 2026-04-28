using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;

namespace Warehouse.Service;

public class DetailsMoliendaService : IDetailsMoliendaService
{
    private readonly DbWarehouseContext _context;
    private readonly ILogger<DetailsMoliendaService> _logger;

    public DetailsMoliendaService(DbWarehouseContext context, ILogger<DetailsMoliendaService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<DetailsMoliendaDelison>> GetByMoliendaAsync(int idMolienda, string type)
    {
        try
        {
            return await _context.DetailsMoliendaDelison
                .Where(d => d.IdMolienda == idMolienda && d.Type == type.ToUpper() && d.Active)
                .OrderBy(d => d.Fecha)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving details for molienda {IdMolienda} type {Type}", idMolienda, type);
            throw;
        }
    }

    public async Task<DetailsMoliendaDelison?> GetByIdAsync(int id)
    {
        try
        {
            return await _context.DetailsMoliendaDelison
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id && d.Active);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving detail molienda with ID {Id}", id);
            throw;
        }
    }

    public async Task<DetailsMoliendaDelison> CreateAsync(DetailsMoliendaDelison detail)
    {
        try
        {
            detail.Type = detail.Type.ToUpper();
            _context.DetailsMoliendaDelison.Add(detail);
            await _context.SaveChangesAsync();

            await UpdateParentCountAsync(detail.IdMolienda);
            return detail;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating detail molienda");
            throw;
        }
    }

    public async Task<DetailsMoliendaDelison?> UpdateAsync(int id, DetailsMoliendaDelison detail)
    {
        try
        {
            var existing = await _context.DetailsMoliendaDelison.FindAsync(id);
            if (existing == null) return null;

            existing.Fecha     = detail.Fecha;
            existing.Cantidad  = detail.Cantidad;
            existing.Type      = detail.Type.ToUpper();
            existing.IdCatalog = detail.IdCatalog;

            await _context.SaveChangesAsync();
            return existing;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating detail molienda with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var existing = await _context.DetailsMoliendaDelison.FindAsync(id);
            if (existing == null) return false;

            existing.Active = false;
            await _context.SaveChangesAsync();

            await UpdateParentCountAsync(existing.IdMolienda);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting detail molienda with ID {Id}", id);
            throw;
        }
    }

    private async Task UpdateParentCountAsync(int idMolienda)
    {
        var parent = await _context.MoliendaDelison.FindAsync(idMolienda);
        if (parent == null) return;

        parent.Entradas = await _context.DetailsMoliendaDelison
            .CountAsync(d => d.IdMolienda == idMolienda && d.Type == "ENTRADA" && d.Active);

        parent.Salidas = await _context.DetailsMoliendaDelison
            .CountAsync(d => d.IdMolienda == idMolienda && d.Type == "SALIDA" && d.Active);

        var sumEntradas = await _context.DetailsMoliendaDelison
            .Where(d => d.IdMolienda == idMolienda && d.Type == "ENTRADA" && d.Active)
            .SumAsync(d => (decimal?)d.Cantidad) ?? 0;

        var sumSalidas = await _context.DetailsMoliendaDelison
            .Where(d => d.IdMolienda == idMolienda && d.Type == "SALIDA" && d.Active)
            .SumAsync(d => (decimal?)d.Cantidad) ?? 0;

        parent.TotalEntradas    = sumEntradas;
        parent.TotalSalidas     = sumSalidas;
        parent.TotalInventarios = (int)(sumEntradas - sumSalidas);
        parent.DateModified     = DateTime.Now;

        await _context.SaveChangesAsync();
    }
}

public interface IDetailsMoliendaService
{
    Task<List<DetailsMoliendaDelison>> GetByMoliendaAsync(int idMolienda, string type);
    Task<DetailsMoliendaDelison?> GetByIdAsync(int id);
    Task<DetailsMoliendaDelison> CreateAsync(DetailsMoliendaDelison detail);
    Task<DetailsMoliendaDelison?> UpdateAsync(int id, DetailsMoliendaDelison detail);
    Task<bool> DeleteAsync(int id);
}
