using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;

namespace Warehouse.Service.Delison
{
    public interface IMaterialXModuloService
    {
        Task<List<MaterialXModulo>> GetAll(int idCompany);
        Task<List<MaterialXModulo>> GetByType(int idCompany, string type);
        Task<MaterialXModulo?> GetById(int id);
        Task<MaterialXModulo> Create(MaterialXModulo entity);
        Task<MaterialXModulo?> Update(int id, MaterialXModulo entity);
        Task<bool> Delete(int id);
    }

    public class MaterialXModuloService : IMaterialXModuloService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<MaterialXModuloService> _logger;

        public MaterialXModuloService(DbWarehouseContext context, ILogger<MaterialXModuloService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<MaterialXModulo>> GetAll(int idCompany)
        {
            return await _context.MaterialXModulos
                .Where(m => m.Active && m.IdCompany == idCompany)
                .OrderBy(m => m.Type)
                .ToListAsync();
        }

        public async Task<List<MaterialXModulo>> GetByType(int idCompany, string type)
        {
            return await _context.MaterialXModulos
                .Where(m => m.Active && m.IdCompany == idCompany && m.Type == type)
                .ToListAsync();
        }

        public async Task<MaterialXModulo?> GetById(int id)
        {
            return await _context.MaterialXModulos.FindAsync(id);
        }

        public async Task<MaterialXModulo> Create(MaterialXModulo entity)
        {
            entity.Active = true;
            _context.MaterialXModulos.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<MaterialXModulo?> Update(int id, MaterialXModulo entity)
        {
            var existing = await _context.MaterialXModulos.FindAsync(id);
            if (existing == null) return null;

            existing.IdCompany  = entity.IdCompany;
            existing.IdArticulo = entity.IdArticulo;
            existing.Cantidad   = entity.Cantidad;
            existing.Type       = entity.Type;
            existing.Active     = entity.Active;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> Delete(int id)
        {
            var existing = await _context.MaterialXModulos.FindAsync(id);
            if (existing == null) return false;

            existing.Active = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
