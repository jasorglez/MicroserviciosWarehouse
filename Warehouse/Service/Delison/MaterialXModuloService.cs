using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;

namespace Warehouse.Service.Delison
{
    public interface IMaterialXModuloService
    {
        Task<List<MaterialXModulo>> GetAll(int idCompany);
        Task<List<MaterialXModulo>> GetByType(int idCompany, string type);
        Task<List<MaterialXModulo>> GetByCatalog(int idCompany, int idCatalog);
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

        public async Task<List<MaterialXModulo>> GetByCatalog(int idCompany, int idCatalog)
        {
            return await _context.MaterialXModulos
                .Where(m => m.IdCompany == idCompany && m.IdCatalog == idCatalog)
                .ToListAsync();
        }

        public async Task<MaterialXModulo?> GetById(int id)
        {
            return await _context.MaterialXModulos.FindAsync(id);
        }

        public async Task<MaterialXModulo> Create(MaterialXModulo entity)
        {
            entity.Active = true;

            // Auto-numeración para botes de catálogo (id_catalog != null, id_articulo == null)
            if (entity.IdCatalog != null && entity.IdArticulo == null)
            {
                var maxNum = await _context.MaterialXModulos
                    .Where(m => m.Active
                             && m.IdCatalog      == entity.IdCatalog
                             && m.IdPrefijoFase  == entity.IdPrefijoFase
                             && m.IdMatPrima     == entity.IdMatPrima
                             && m.NumBote        != null)
                    .Select(m => m.NumBote)
                    .MaxAsync(n => (int?)n) ?? 0;

                entity.NumBote = maxNum + 1;
            }

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
            existing.IdCatalog  = entity.IdCatalog;
            existing.EditBultos  = entity.EditBultos;
            existing.Active     = entity.Active;
            existing.Molienda      = entity.Molienda;
            existing.IdMatPrima    = entity.IdMatPrima;
            existing.IdPrefijoFase = entity.IdPrefijoFase;
            existing.Prefijo       = entity.Prefijo;
            existing.NumBote       = entity.NumBote;

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
