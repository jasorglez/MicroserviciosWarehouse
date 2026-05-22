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
                .Where(m => m.IdCompany == idCompany);

            if (!string.IsNullOrEmpty(type))
                query = query.Where(m => m.Type == type);

            query = query.OrderByDescending(m => m.Active)
                         .ThenByDescending(m => m.DateModified);

            var items = await query.AsNoTracking().ToListAsync();

            var ids = items.Select(m => m.Id).ToList();
            if (ids.Count == 0) return items;

            var entradaCounts = await _context.DetailsMoliendaDelison
                .Where(d => ids.Contains(d.IdMolienda) && d.Type == "ENTRADA" && d.Active)
                .GroupBy(d => d.IdMolienda)
                .Select(g => new { IdMolienda = g.Key, Count = g.Count() })
                .ToListAsync();

            var salidaCounts = await _context.DetailsMoliendaDelison
                .Where(d => ids.Contains(d.IdMolienda) && d.Type == "SALIDA" && d.Active)
                .GroupBy(d => d.IdMolienda)
                .Select(g => new { IdMolienda = g.Key, Count = g.Count() })
                .ToListAsync();

            var entradaMap = entradaCounts.ToDictionary(x => x.IdMolienda, x => x.Count);
            var salidaMap  = salidaCounts.ToDictionary(x => x.IdMolienda, x => x.Count);

            foreach (var item in items)
            {
                item.Entradas = entradaMap.TryGetValue(item.Id, out var e) ? e : 0;
                item.Salidas  = salidaMap.TryGetValue(item.Id, out var s)  ? s : 0;
            }

            return items;
        }

        public async Task<MoliendaDelison?> GetById(int id)
        {
            var item = await _context.MoliendaDelison
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (item == null) return null;

            item.Entradas = await _context.DetailsMoliendaDelison
                .CountAsync(d => d.IdMolienda == id && d.Type == "ENTRADA" && d.Active);
            item.Salidas = await _context.DetailsMoliendaDelison
                .CountAsync(d => d.IdMolienda == id && d.Type == "SALIDA" && d.Active);

            return item;
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

            if (existing == null)
                return false;

            _context.MoliendaDelison.Remove(existing);

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
