using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;

namespace Warehouse.Service.Delison
{
    public interface IPesoVolumenService
    {
        Task<List<PesoVolumenDelison>> GetByCompany(int idCompany);
        Task<PesoVolumenDelison> Create(PesoVolumenDelison data);
        Task<PesoVolumenDelison?> Update(int id, PesoVolumenDelison data);
        Task<bool> Delete(int id);
    }

    public class PesoVolumenService : IPesoVolumenService
    {
        private readonly DbWarehouseContext _context;

        public PesoVolumenService(DbWarehouseContext context)
        {
            _context = context;
        }

        public async Task<List<PesoVolumenDelison>> GetByCompany(int idCompany)
        {
            return await _context.PesosVolumenes
                .Where(p => p.IdCompany == idCompany)
                .OrderByDescending(p => p.Active)
                .ThenBy(p => p.Abreviatura)
                .ToListAsync();
        }

        public async Task<PesoVolumenDelison> Create(PesoVolumenDelison data)
        {
            data.DateModified = DateTime.Now;
            _context.PesosVolumenes.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<PesoVolumenDelison?> Update(int id, PesoVolumenDelison data)
        {
            var existing = await _context.PesosVolumenes.FindAsync(id);
            if (existing == null) return null;

            existing.Abreviatura  = data.Abreviatura;
            existing.Nombre       = data.Nombre;
            existing.Tipo         = data.Tipo;
            existing.FactorBase   = data.FactorBase;
            existing.Active       = data.Active;
            existing.DateModified = DateTime.Now;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> Delete(int id)
        {
            var existing = await _context.PesosVolumenes.FindAsync(id);
            if (existing == null) return false;

            _context.PesosVolumenes.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
