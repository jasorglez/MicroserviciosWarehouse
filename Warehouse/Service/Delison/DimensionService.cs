using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;

namespace Warehouse.Service.Delison
{
    public interface IDimensionService
    {
        Task<List<DimensionDelison>> GetByCompany(int idCompany);
        Task<DimensionDelison> Create(DimensionDelison data);
        Task<DimensionDelison?> Update(int id, DimensionDelison data);
        Task<bool> Delete(int id);
    }

    public class DimensionService : IDimensionService
    {
        private readonly DbWarehouseContext _context;

        public DimensionService(DbWarehouseContext context)
        {
            _context = context;
        }

        public async Task<List<DimensionDelison>> GetByCompany(int idCompany)
        {
            return await _context.Dimensiones
                .Where(d => d.IdCompany == idCompany)
                .OrderByDescending(d => d.Active)
                .ThenBy(d => d.Nombre)
                .ToListAsync();
        }

        public async Task<DimensionDelison> Create(DimensionDelison data)
        {
            data.DateModified = DateTime.Now;
            _context.Dimensiones.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<DimensionDelison?> Update(int id, DimensionDelison data)
        {
            var existing = await _context.Dimensiones.FindAsync(id);
            if (existing == null) return null;

            existing.Nombre       = data.Nombre;
            existing.Active       = data.Active;
            existing.DateModified = DateTime.Now;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> Delete(int id)
        {
            var existing = await _context.Dimensiones.FindAsync(id);
            if (existing == null) return false;

            _context.Dimensiones.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
