using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;

namespace Warehouse.Service.Delison
{
    public interface ICaracteristicasMateriaPrimaService
    {
        Task<List<CaracteristicasMateriaPrimaDelison>> GetByMaterial(int idMaterial);
        Task<CaracteristicasMateriaPrimaDelison?> GetById(int id);
        Task<CaracteristicasMateriaPrimaDelison> Create(CaracteristicasMateriaPrimaDelison data);
        Task<CaracteristicasMateriaPrimaDelison?> Update(int id, CaracteristicasMateriaPrimaDelison data);
        Task<bool> Delete(int id);
    }

    public class CaracteristicasMateriaPrimaService : ICaracteristicasMateriaPrimaService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<CaracteristicasMateriaPrimaService> _logger;

        public CaracteristicasMateriaPrimaService(DbWarehouseContext context, ILogger<CaracteristicasMateriaPrimaService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        public async Task<List<CaracteristicasMateriaPrimaDelison>> GetByMaterial(int idMaterial)
        {
            return await _context.CaracteristicasMateriaPrima
                .Where(c => c.IdMaterial == idMaterial && c.Active)
                .OrderBy(c => c.Id)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<CaracteristicasMateriaPrimaDelison?> GetById(int id)
        {
            return await _context.CaracteristicasMateriaPrima
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<CaracteristicasMateriaPrimaDelison> Create(CaracteristicasMateriaPrimaDelison data)
        {
            data.Active       = true;
            data.Caracteristica = data.Caracteristica?.Trim().ToUpperInvariant();
            data.DateModified = DateTime.UtcNow;
            _context.CaracteristicasMateriaPrima.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<CaracteristicasMateriaPrimaDelison?> Update(int id, CaracteristicasMateriaPrimaDelison data)
        {
            var existing = await _context.CaracteristicasMateriaPrima.FindAsync(id);
            if (existing == null) return null;

            existing.Activo         = data.Activo;
            existing.Caracteristica = data.Caracteristica?.Trim().ToUpperInvariant();
            existing.DateModified   = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> Delete(int id)
        {
            var existing = await _context.CaracteristicasMateriaPrima.FindAsync(id);
            if (existing == null) return false;
            existing.Active       = false;
            existing.DateModified = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
