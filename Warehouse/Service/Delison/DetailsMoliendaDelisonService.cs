using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;

namespace Warehouse.Service.Delison
{
    public class DetailsMoliendaDelisonService : IDetailsMoliendaDelisonService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<DetailsMoliendaDelisonService> _logger;

        public DetailsMoliendaDelisonService(DbWarehouseContext context, ILogger<DetailsMoliendaDelisonService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        public async Task<List<DetailsMoliendaDelison>> GetByMolienda(int idMolienda, string? type)
        {
            var query = _context.DetailsMoliendaDelison
                .Where(d => d.IdMolienda == idMolienda && d.Active);

            if (!string.IsNullOrEmpty(type))
                query = query.Where(d => d.Type == type);

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<DetailsMoliendaDelison> Create(DetailsMoliendaDelison data)
        {
            data.Active = true;
            _context.DetailsMoliendaDelison.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<DetailsMoliendaDelison?> Update(int id, DetailsMoliendaDelison data)
        {
            var existing = await _context.DetailsMoliendaDelison.FindAsync(id);
            if (existing == null) return null;

            existing.Type          = data.Type;
            existing.IdRequisition = data.IdRequisition;
            existing.CantidadReq   = data.CantidadReq;
            existing.NumCantidadOc = data.NumCantidadOc;
            existing.Fecha         = data.Fecha;
            existing.Cantidad      = data.Cantidad;
            existing.IdCatalog     = data.IdCatalog;
            existing.Active        = data.Active;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> Delete(int id)
        {
            var existing = await _context.DetailsMoliendaDelison.FindAsync(id);
            if (existing == null) return false;
            existing.Active = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }

    public interface IDetailsMoliendaDelisonService
    {
        Task<List<DetailsMoliendaDelison>> GetByMolienda(int idMolienda, string? type);
        Task<DetailsMoliendaDelison> Create(DetailsMoliendaDelison data);
        Task<DetailsMoliendaDelison?> Update(int id, DetailsMoliendaDelison data);
        Task<bool> Delete(int id);
    }
}
