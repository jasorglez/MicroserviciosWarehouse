using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;

namespace Warehouse.Service.Delison
{
    public interface IDatosExternosMoliendaService
    {
        Task<List<DatosExternosMoliendaDelison>> GetByEntrada(int idEntrada);
        Task<DatosExternosMoliendaDelison?> GetById(int id);
        Task<DatosExternosMoliendaDelison> Create(DatosExternosMoliendaDelison data);
        Task<DatosExternosMoliendaDelison?> Update(int id, DatosExternosMoliendaDelison data);
        Task<bool> Delete(int id);
    }

    public class DatosExternosMoliendaService : IDatosExternosMoliendaService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<DatosExternosMoliendaService> _logger;

        public DatosExternosMoliendaService(DbWarehouseContext context, ILogger<DatosExternosMoliendaService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        public async Task<List<DatosExternosMoliendaDelison>> GetByEntrada(int idEntrada)
        {
            return await _context.DatosExternosMolienda
                .Where(d => d.IdEntrada == idEntrada && d.Active)
                .OrderBy(d => d.Id)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<DatosExternosMoliendaDelison?> GetById(int id)
        {
            return await _context.DatosExternosMolienda
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<DatosExternosMoliendaDelison> Create(DatosExternosMoliendaDelison data)
        {
            data.Active       = true;
            data.DateModified = DateTime.UtcNow;
            _context.DatosExternosMolienda.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<DatosExternosMoliendaDelison?> Update(int id, DatosExternosMoliendaDelison data)
        {
            var existing = await _context.DatosExternosMolienda.FindAsync(id);
            if (existing == null) return null;

            existing.Lote           = data.Lote;
            existing.CaducidadMeses = data.CaducidadMeses;
            existing.CantidadXLote  = data.CantidadXLote;
            existing.DateModified   = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        // Borrado FÍSICO: las filas de "datos externos" se sincronizan con el grid versión-gasto;
        // si una se elimina del grid, se quita realmente de la BD.
        public async Task<bool> Delete(int id)
        {
            var existing = await _context.DatosExternosMolienda.FindAsync(id);
            if (existing == null) return false;
            _context.DatosExternosMolienda.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
