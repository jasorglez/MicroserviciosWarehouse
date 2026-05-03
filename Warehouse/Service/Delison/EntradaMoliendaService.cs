using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;

namespace Warehouse.Service.Delison
{
    public interface IEntradaMoliendaService
    {
        Task<List<EntradaMoliendaDelison>> GetByOc(int idOc);
        Task<List<EntradaMoliendaDelison>> GetByOcAndMaterial(int idOc, int idMaterial);
        Task<EntradaMoliendaDelison?> GetById(int id);
        Task<EntradaMoliendaDelison> Create(EntradaMoliendaDelison data);
        Task<EntradaMoliendaDelison?> Update(int id, EntradaMoliendaDelison data);
        Task<bool> Delete(int id);
    }

    public class EntradaMoliendaService : IEntradaMoliendaService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<EntradaMoliendaService> _logger;

        public EntradaMoliendaService(DbWarehouseContext context, ILogger<EntradaMoliendaService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        public async Task<List<EntradaMoliendaDelison>> GetByOc(int idOc)
        {
            return await _context.EntradasMolienda
                .Where(e => e.IdOc == idOc && e.Active)
                .OrderBy(e => e.FechaRecepcion)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<EntradaMoliendaDelison>> GetByOcAndMaterial(int idOc, int idMaterial)
        {
            return await _context.EntradasMolienda
                .Where(e => e.IdOc == idOc && e.Active
                         && (e.IdMaterial == null || e.IdMaterial == idMaterial))
                .OrderBy(e => e.FechaRecepcion)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<EntradaMoliendaDelison?> GetById(int id)
        {
            return await _context.EntradasMolienda
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<EntradaMoliendaDelison> Create(EntradaMoliendaDelison data)
        {
            data.Active       = true;
            data.DateModified = DateTime.UtcNow;
            _context.EntradasMolienda.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<EntradaMoliendaDelison?> Update(int id, EntradaMoliendaDelison data)
        {
            var existing = await _context.EntradasMolienda.FindAsync(id);
            if (existing == null) return null;

            existing.FechaRecepcion  = data.FechaRecepcion;
            existing.CantidadEntrada = data.CantidadEntrada;
            existing.Bultos          = data.Bultos;
            existing.RevisionConfigu = data.RevisionConfigu;
            existing.Pago            = data.Pago;
            existing.Usuario         = data.Usuario;
            existing.Liberacion      = data.Liberacion;
            existing.Active          = data.Active;
            existing.DateModified    = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> Delete(int id)
        {
            var existing = await _context.EntradasMolienda.FindAsync(id);
            if (existing == null) return false;
            existing.Active       = false;
            existing.DateModified = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
