using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;

namespace Warehouse.Service.Delison
{
    public interface IAutorizacionMontoService
    {
        Task<List<AutorizacionMonto>> GetByCompany(int idCompany);
        Task<AutorizacionMonto?> GetById(int id);
        Task<AutorizacionMonto> Create(AutorizacionMonto data);
        Task<AutorizacionMonto?> Update(int id, AutorizacionMonto data);
    }

    public class AutorizacionMontoService : IAutorizacionMontoService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<AutorizacionMontoService> _logger;

        public AutorizacionMontoService(DbWarehouseContext context, ILogger<AutorizacionMontoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<AutorizacionMonto>> GetByCompany(int idCompany)
        {
            return await _context.AutorizacionMontos
                .Where(a => a.IdCompany == idCompany && a.Active)
                .OrderBy(a => a.Nivel)
                .ToListAsync();
        }

        public async Task<AutorizacionMonto?> GetById(int id)
        {
            return await _context.AutorizacionMontos.FindAsync(id);
        }

        public async Task<AutorizacionMonto> Create(AutorizacionMonto data)
        {
            _context.AutorizacionMontos.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<AutorizacionMonto?> Update(int id, AutorizacionMonto data)
        {
            var existing = await _context.AutorizacionMontos.FindAsync(id);
            if (existing == null) return null;

            existing.MontoMin    = data.MontoMin;
            existing.MontoMax    = data.MontoMax;
            existing.Descripcion = data.Descripcion;
            existing.Active      = data.Active;

            await _context.SaveChangesAsync();
            return existing;
        }
    }
}
