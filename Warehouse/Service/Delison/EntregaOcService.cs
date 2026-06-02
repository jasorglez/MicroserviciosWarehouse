using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;

namespace Warehouse.Service.Delison
{
    public interface IEntregaOcService
    {
        Task<List<EntregaOcDelison>> GetByDetail(int idDetailsreqoc);
        Task<EntregaOcDelison?> GetById(int id);
        Task<EntregaOcDelison> Create(EntregaOcDelison data);
        Task<EntregaOcDelison?> Update(int id, EntregaOcDelison data);
        Task<bool> Delete(int id);
    }

    public class EntregaOcService : IEntregaOcService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<EntregaOcService> _logger;

        public EntregaOcService(DbWarehouseContext context, ILogger<EntregaOcService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        public async Task<List<EntregaOcDelison>> GetByDetail(int idDetailsreqoc)
        {
            return await _context.EntregasOc
                .Where(e => e.IdDetailsreqoc == idDetailsreqoc && e.Active)
                .OrderBy(e => e.Id)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<EntregaOcDelison?> GetById(int id)
        {
            return await _context.EntregasOc
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<EntregaOcDelison> Create(EntregaOcDelison data)
        {
            data.Active       = true;
            data.DateModified = DateTime.UtcNow;
            _context.EntregasOc.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<EntregaOcDelison?> Update(int id, EntregaOcDelison data)
        {
            var existing = await _context.EntregasOc.FindAsync(id);
            if (existing == null) return null;

            existing.FechaEntrega        = data.FechaEntrega;
            existing.CantidadRecibir     = data.CantidadRecibir;
            existing.NotaFactura         = data.NotaFactura;
            existing.TotalEntrega        = data.TotalEntrega;
            existing.FechaEntradaAlmacen = data.FechaEntradaAlmacen;
            existing.Close               = data.Close;
            existing.Active              = data.Active;
            existing.DateModified        = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> Delete(int id)
        {
            var existing = await _context.EntregasOc.FindAsync(id);
            if (existing == null) return false;
            // Hard delete: la fila desaparece físicamente. No hay FKs apuntando
            // a Delison.entregas_oc, así que es seguro.
            _context.EntregasOc.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
