using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;

namespace Warehouse.Service.Delison
{
    public interface ICondicionPagoService
    {
        Task<List<CondicionPago>> GetByCompany(int idCompany);
        Task<CondicionPago> Create(CondicionPago data);
        Task<CondicionPago?> Update(int id, CondicionPago data);
        Task<bool> Delete(int id);
    }

    public class CondicionPagoService : ICondicionPagoService
    {
        private readonly DbWarehouseContext _context;

        public CondicionPagoService(DbWarehouseContext context)
        {
            _context = context;
        }

        public async Task<List<CondicionPago>> GetByCompany(int idCompany)
        {
            return await _context.CondicionesPago
                .Where(c => c.IdCompany == idCompany)
                .OrderByDescending(c => c.Active)
                .ThenBy(c => c.Descripcion)
                .ToListAsync();
        }

        public async Task<CondicionPago> Create(CondicionPago data)
        {
            data.DateModified = DateTime.Now;
            _context.CondicionesPago.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<CondicionPago?> Update(int id, CondicionPago data)
        {
            var existing = await _context.CondicionesPago.FindAsync(id);
            if (existing == null) return null;

            existing.Descripcion  = data.Descripcion;
            existing.Cantidad     = data.Cantidad;
            existing.Active       = data.Active;
            existing.DateModified = DateTime.Now;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> Delete(int id)
        {
            var existing = await _context.CondicionesPago.FindAsync(id);
            if (existing == null) return false;

            _context.CondicionesPago.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
