using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;

namespace Warehouse.Service.Delison
{
    public interface IRevisionCaracteristicasEntradaService
    {
        Task<List<RevisionCaracteristicasEntradaDelison>> GetByEntrada(int idEntrada);
        Task<RevisionCaracteristicasEntradaDelison?> GetById(int id);
        Task<RevisionCaracteristicasEntradaDelison> Create(RevisionCaracteristicasEntradaDelison data);
        Task<RevisionCaracteristicasEntradaDelison?> Update(int id, RevisionCaracteristicasEntradaDelison data);
        Task<bool> Delete(int id);
    }

    public class RevisionCaracteristicasEntradaService : IRevisionCaracteristicasEntradaService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<RevisionCaracteristicasEntradaService> _logger;

        public RevisionCaracteristicasEntradaService(DbWarehouseContext context, ILogger<RevisionCaracteristicasEntradaService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        public async Task<List<RevisionCaracteristicasEntradaDelison>> GetByEntrada(int idEntrada)
        {
            return await _context.RevisionCaracteristicasEntrada
                .Where(r => r.IdEntrada == idEntrada && r.Active)
                .OrderBy(r => r.Id)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<RevisionCaracteristicasEntradaDelison?> GetById(int id)
        {
            return await _context.RevisionCaracteristicasEntrada
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<RevisionCaracteristicasEntradaDelison> Create(RevisionCaracteristicasEntradaDelison data)
        {
            data.Active       = true;
            data.Comentarios  = data.Comentarios?.Trim().ToUpperInvariant();
            data.DateModified = DateTime.UtcNow;
            _context.RevisionCaracteristicasEntrada.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<RevisionCaracteristicasEntradaDelison?> Update(int id, RevisionCaracteristicasEntradaDelison data)
        {
            var existing = await _context.RevisionCaracteristicasEntrada.FindAsync(id);
            if (existing == null) return null;

            existing.Reviso       = data.Reviso;
            existing.Comentarios  = data.Comentarios?.Trim().ToUpperInvariant();
            existing.IdTrabajador = data.IdTrabajador;
            existing.DateModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> Delete(int id)
        {
            var existing = await _context.RevisionCaracteristicasEntrada.FindAsync(id);
            if (existing == null) return false;
            existing.Active       = false;
            existing.DateModified = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
