using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;

namespace Warehouse.Service.Delison
{
    public class MoliendaDelisonService : IMoliendaDelisonService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<MoliendaDelisonService> _logger;

        public MoliendaDelisonService(DbWarehouseContext context, ILogger<MoliendaDelisonService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        public async Task<List<MoliendaDelison>> GetByCompany(int idCompany, string? type = null)
        {
            var query = _context.MoliendaDelison
                .Where(m => m.IdCompany == idCompany && m.Active);

            if (!string.IsNullOrEmpty(type))
                query = query.Where(m => m.Type == type);

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<MoliendaDelison?> GetById(int id)
        {
            return await _context.MoliendaDelison
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<MoliendaDelison> Create(MoliendaDelison data)
        {
            data.DateModified = DateTime.UtcNow;
            data.Active = true;
            _context.MoliendaDelison.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<MoliendaDelison?> Update(int id, MoliendaDelison data)
        {
            var existing = await _context.MoliendaDelison.FindAsync(id);
            if (existing == null) return null;

            existing.IdSucursal         = data.IdSucursal;
            existing.IdMaterial         = data.IdMaterial;
            existing.Entradas           = data.Entradas;
            existing.Salidas            = data.Salidas;
            existing.TotalInventarios   = data.TotalInventarios;
            existing.AjustesInventarios = data.AjustesInventarios;
            existing.TotalEntradas      = data.TotalEntradas;
            existing.TotalSalidas       = data.TotalSalidas;
            existing.Type               = data.Type;
            existing.Comentarios        = data.Comentarios;
            existing.Active             = data.Active;
            existing.DateModified       = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> Delete(int id)
        {
            var existing = await _context.MoliendaDelison.FindAsync(id);
            if (existing == null) return false;
            existing.Active = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }

    public interface IMoliendaDelisonService
    {
        Task<List<MoliendaDelison>> GetByCompany(int idCompany, string? type = null);
        Task<MoliendaDelison?> GetById(int id);
        Task<MoliendaDelison> Create(MoliendaDelison data);
        Task<MoliendaDelison?> Update(int id, MoliendaDelison data);
        Task<bool> Delete(int id);
    }
}
