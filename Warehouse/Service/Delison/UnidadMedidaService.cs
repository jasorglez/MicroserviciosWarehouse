using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;

namespace Warehouse.Service.Delison
{
    public interface IUnidadMedidaService
    {
        Task<List<UnidadMedidaDelison>> GetByCompany(int idCompany);
        Task<UnidadMedidaDelison> Create(UnidadMedidaDelison data);
        Task<UnidadMedidaDelison?> Update(int id, UnidadMedidaDelison data);
        Task<bool> Delete(int id);
    }

    public class UnidadMedidaService : IUnidadMedidaService
    {
        private readonly DbWarehouseContext _context;

        public UnidadMedidaService(DbWarehouseContext context)
        {
            _context = context;
        }

        public async Task<List<UnidadMedidaDelison>> GetByCompany(int idCompany)
        {
            return await _context.UnidadesMedida
                .Where(u => u.IdCompany == idCompany)
                .OrderByDescending(u => u.Active)
                .ThenBy(u => u.Abreviatura)
                .ToListAsync();
        }

        public async Task<UnidadMedidaDelison> Create(UnidadMedidaDelison data)
        {
            data.DateModified = DateTime.Now;
            _context.UnidadesMedida.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<UnidadMedidaDelison?> Update(int id, UnidadMedidaDelison data)
        {
            var existing = await _context.UnidadesMedida.FindAsync(id);
            if (existing == null) return null;

            existing.Abreviatura  = data.Abreviatura;
            existing.Nombre       = data.Nombre;
            existing.Active       = data.Active;
            existing.DateModified = DateTime.Now;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> Delete(int id)
        {
            var existing = await _context.UnidadesMedida.FindAsync(id);
            if (existing == null) return false;

            _context.UnidadesMedida.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
