using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;

namespace Warehouse.Service;

public class MoliendaService : IMoliendaService
{
    private readonly DbWarehouseContext _context;
    private readonly ILogger<MoliendaService> _logger;

    public MoliendaService(DbWarehouseContext context, ILogger<MoliendaService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<object>> GetAllAsync(int idCompany)
    {
        try
        {
            return await _context.MoliendaDelison
                .Where(m => m.Active && m.IdCompany == idCompany)
                .OrderByDescending(m => m.DateModified)
                .Select(m => (object)new
                {
                    m.Id,
                    m.IdCompany,
                    m.IdSucursal,
                    m.IdMaterial,
                    Entradas = _context.DetailsMoliendaDelison
                        .Count(d => d.IdMolienda == m.Id && d.Type == "ENTRADA" && d.Active),
                    Salidas = _context.DetailsMoliendaDelison
                        .Count(d => d.IdMolienda == m.Id && d.Type == "SALIDA" && d.Active),
                    TotalEntradas = _context.DetailsMoliendaDelison
                        .Where(d => d.IdMolienda == m.Id && d.Type == "ENTRADA" && d.Active)
                        .Sum(d => (decimal?)d.Cantidad) ?? 0,
                    TotalSalidas = _context.DetailsMoliendaDelison
                        .Where(d => d.IdMolienda == m.Id && d.Type == "SALIDA" && d.Active)
                        .Sum(d => (decimal?)d.Cantidad) ?? 0,
                    TotalInventarios = (int?)(
                        _context.DetailsMoliendaDelison
                            .Where(d => d.IdMolienda == m.Id && d.Type == "ENTRADA" && d.Active)
                            .Sum(d => (decimal?)d.Cantidad) -
                        _context.DetailsMoliendaDelison
                            .Where(d => d.IdMolienda == m.Id && d.Type == "SALIDA" && d.Active)
                            .Sum(d => (decimal?)d.Cantidad)),
                    m.AjustesInventarios,
                    m.Comentarios,
                    m.DateModified,
                    m.Active
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving molienda records for company {IdCompany}", idCompany);
            throw;
        }
    }

    public async Task<MoliendaDelison?> GetByIdAsync(int id)
    {
        try
        {
            return await _context.MoliendaDelison
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id && m.Active);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving molienda with ID {Id}", id);
            throw;
        }
    }

    public async Task<MoliendaDelison> CreateAsync(MoliendaDelison molienda)
    {
        try
        {
            molienda.DateModified = DateTime.Now;
            _context.MoliendaDelison.Add(molienda);
            await _context.SaveChangesAsync();
            return molienda;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating molienda record");
            throw;
        }
    }

    public async Task<MoliendaDelison?> UpdateAsync(int id, MoliendaDelison molienda)
    {
        try
        {
            var existing = await _context.MoliendaDelison.FindAsync(id);
            if (existing == null) return null;

            existing.IdSucursal         = molienda.IdSucursal;
            existing.IdMaterial         = molienda.IdMaterial;
            existing.AjustesInventarios = molienda.AjustesInventarios;
            existing.Comentarios        = molienda.Comentarios;
            existing.Active             = molienda.Active;
            existing.DateModified       = DateTime.Now;

            await _context.SaveChangesAsync();
            return existing;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating molienda with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var existing = await _context.MoliendaDelison.FindAsync(id);
            if (existing == null) return false;

            existing.Active = false;
            existing.DateModified = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting molienda with ID {Id}", id);
            throw;
        }
    }
}

public interface IMoliendaService
{
    Task<List<object>> GetAllAsync(int idCompany);
    Task<MoliendaDelison?> GetByIdAsync(int id);
    Task<MoliendaDelison> CreateAsync(MoliendaDelison molienda);
    Task<MoliendaDelison?> UpdateAsync(int id, MoliendaDelison molienda);
    Task<bool> DeleteAsync(int id);
}
