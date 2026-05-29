using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;

namespace Warehouse.Service.Delison
{
    public interface IExtractionFermentationCatalogService
    {
        Task<List<ExtractionFermentationCatalog>> GetAll(int idCompany);
        Task<List<ExtractionFermentationCatalog>> GetByBranch(int idCompany, int idBranch);
        Task<ExtractionFermentationCatalog?> GetById(int id);
        Task<ExtractionFermentationCatalog> Create(ExtractionFermentationCatalog entity);
        Task<ExtractionFermentationCatalog?> Update(int id, ExtractionFermentationCatalog entity);
        Task<bool> Delete(int id);
    }

    public class ExtractionFermentationCatalogService : IExtractionFermentationCatalogService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<ExtractionFermentationCatalogService> _logger;

        public ExtractionFermentationCatalogService(
            DbWarehouseContext context,
            ILogger<ExtractionFermentationCatalogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ExtractionFermentationCatalog>> GetAll(int idCompany)
        {
            return await _context.ExtractionFermentationCatalogs
                .Where(x => x.Active && x.IdCompany == idCompany)
                .OrderBy(x => x.Description)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<ExtractionFermentationCatalog>> GetByBranch(int idCompany, int idBranch)
        {
            // TODO: Una vez que la columna id_branch exista en la BD, cambiar a:
            // .Where(x => x.Active && x.IdCompany == idCompany && (x.IdBranch == idBranch || x.IdBranch == null))
            return await _context.ExtractionFermentationCatalogs
                .Where(x => x.Active && x.IdCompany == idCompany)
                .OrderBy(x => x.Description)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ExtractionFermentationCatalog?> GetById(int id)
        {
            return await _context.ExtractionFermentationCatalogs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<ExtractionFermentationCatalog> Create(ExtractionFermentationCatalog entity)
        {
            entity.Description = entity.Description.Trim();
            entity.Active = true;

            _context.ExtractionFermentationCatalogs.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<ExtractionFermentationCatalog?> Update(int id, ExtractionFermentationCatalog entity)
        {
            var existing = await _context.ExtractionFermentationCatalogs.FindAsync(id);
            if (existing == null) return null;

            existing.IdCompany = entity.IdCompany;
            // TODO: Descomentar cuando la columna id_branch exista en la BD
            // existing.IdBranch = entity.IdBranch;
            existing.Description = entity.Description.Trim();
            existing.Active = entity.Active;
            existing.Molienda = entity.Molienda;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> Delete(int id)
        {
            var existing = await _context.ExtractionFermentationCatalogs.FindAsync(id);
            if (existing == null) return false;

            existing.Active = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
