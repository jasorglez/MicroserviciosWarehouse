using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;

namespace Warehouse.Service.Delison
{
    public interface IDescripcionEmpaqueService
    {
        Task<List<DescripcionEmpaqueDelison>> GetByCompany(int idCompany);
        Task<DescripcionEmpaqueDelison> Create(DescripcionEmpaqueDelison data);
        Task<DescripcionEmpaqueDelison?> Update(int id, DescripcionEmpaqueDelison data);
        Task<bool> Delete(int id);
    }

    public class DescripcionEmpaqueService : IDescripcionEmpaqueService
    {
        private readonly DbWarehouseContext _context;

        public DescripcionEmpaqueService(DbWarehouseContext context)
        {
            _context = context;
        }

        public async Task<List<DescripcionEmpaqueDelison>> GetByCompany(int idCompany)
        {
            return await _context.DescripcionEmpaques
                .Where(d => d.IdCompany == idCompany)
                .OrderByDescending(d => d.Active)
                .ThenBy(d => d.Descripcion)
                .ToListAsync();
        }

        public async Task<DescripcionEmpaqueDelison> Create(DescripcionEmpaqueDelison data)
        {
            data.DateModified = DateTime.Now;
            _context.DescripcionEmpaques.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<DescripcionEmpaqueDelison?> Update(int id, DescripcionEmpaqueDelison data)
        {
            var existing = await _context.DescripcionEmpaques.FindAsync(id);
            if (existing == null) return null;

            existing.Descripcion  = data.Descripcion;
            existing.Active       = data.Active;
            existing.DateModified = DateTime.Now;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> Delete(int id)
        {
            var existing = await _context.DescripcionEmpaques.FindAsync(id);
            if (existing == null) return false;

            _context.DescripcionEmpaques.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
