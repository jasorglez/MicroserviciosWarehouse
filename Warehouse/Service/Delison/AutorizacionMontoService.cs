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
            var existing = await _context.AutorizacionMontos
                .Where(a => a.IdCompany == idCompany && a.Active)
                .OrderBy(a => a.Nivel)
                .ToListAsync();

            if (existing.Count > 0)
                return existing;

            var defaults = new List<AutorizacionMonto>
            {
                new() { IdCompany = idCompany, Nivel = 1, MontoMin = 1m,       MontoMax = 5000m,   Descripcion = "Nivel 1: $1 a $5,000" },
                new() { IdCompany = idCompany, Nivel = 2, MontoMin = 5001m,    MontoMax = 25000m,  Descripcion = "Nivel 2: $5,001 a $25,000" },
                new() { IdCompany = idCompany, Nivel = 3, MontoMin = 25001m,   MontoMax = 100000m, Descripcion = "Nivel 3: $25,001 a $100,000" },
                new() { IdCompany = idCompany, Nivel = 4, MontoMin = 100001m,  MontoMax = null,    Descripcion = "Nivel 4: $100,001 en adelante" }
            };

            _context.AutorizacionMontos.AddRange(defaults);
            await _context.SaveChangesAsync();

            return defaults;
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
