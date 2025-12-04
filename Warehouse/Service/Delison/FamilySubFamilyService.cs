using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehouse.Models.Views;
using Warehouse.Models;
using Warehouse.Models.Delison;

 
namespace Warehouse.Service.Delison
{
    public class FamilySubFamilyDelisonService : IFamilySubFamilyDelisonService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<FamilySubFamilyDelisonService> _logger;

        public FamilySubFamilyDelisonService(DbWarehouseContext context, ILogger<FamilySubFamilyDelisonService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<FamilySubFamilyView>> GetSupplies(int idCompany)
        {
            try
            {
                var items = await _context.FamilySubFamilyViews
                    .Where(f => f.IdCompany == idCompany && f.Subfamilia != null)
                    .OrderBy(f => f.Familia)
                    .ThenBy(f => f.Subfamilia)
                    .ToListAsync();
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplies for company {IdCompany}", idCompany);
                throw;
            }
        }
        public async Task<List<CatalogByMasterFamView>> GetDetailByMaster(int idCompany, int idMasterFamily)
        {
            try
            {
                var items = await _context.CatalogByMasterFamViews
                    .Where(f => f.IdCompanyMaster == idCompany && f.IdMaster == idMasterFamily && f.Subfamilia != null)
                    .OrderBy(f => f.Familia)
                    .ThenBy(f => f.Subfamilia)
                    .ToListAsync();
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplies for company {IdCompany}", idCompany);
                throw;
            }
        }
        public async Task<List<CatalogByMasterFamView>> GetSubFamilyByMaster( int idMasterFamily)
        {
            try
            {
                var items = await _context.CatalogByMasterFamViews
                    .Where(f => f.MasterFamily == idMasterFamily && f.Subfamilia != null && f.Vigente == true)
                    .OrderBy(f => f.Subfamilia)
                    .ToListAsync();
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplies for company {idMasterFamily}", idMasterFamily);
                throw;
            }
        }

        public async Task<List<CatalogByMasterFamView>> GetSubFamilyByMasterVigentes( int idCompany, int idMasterFamily, int idFamilia)
        {
            try
            {
                var existentes = await _context.MateriaByCatalog
                    .Where(m =>m.IdCompany == idCompany && m.Active == true && m.IdConcep == idMasterFamily)
                    .Select(m => m.IdCatalog.Value)
                    .ToListAsync();

                var items = await _context.CatalogByMasterFamViews
                    .Where(f => f.MasterFamily == idFamilia && f.Subfamilia != null && f.Vigente == true && f.IdSubfamily.HasValue && !existentes.Contains(f.IdSubfamily.Value))
                    .OrderBy(f => f.Subfamilia)
                    .ToListAsync();
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplies for company {idMasterFamily}", idMasterFamily);
                throw;
            }
        }

        public async Task<List<MaterialWithCount>> GetArticulosSubFamilyByMaster( int idMasterFamily)
        {
            try
            {
                var items = await _context.MaterialWithCounts
                    .Where(f => f.IdCompany == idMasterFamily && f.Vigente == true )
                    .ToListAsync();
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplies for company {idMasterFamily}", idMasterFamily);
                throw;
            }
        }

        public async Task<List<MaterialWithCount>> GetArticulosSubFamilyByMasterVigentes( int idCompany, int idMasterFamily,int idFamilia)
        {
            try
            {
                var existentes = await _context.MateriaByCatalog
                    .Where(m => m.IdCompany == idCompany && m.Active == true && m.IdConcep == idMasterFamily)
                    .Select(m => m.IdCatalog.Value)
                    .ToListAsync();

                var items = await _context.CatalogByMasterFamViews
                    .Where(f => f.MasterFamily == idFamilia 
                             && f.Subfamilia != null 
                             && f.Vigente == true 
                             && f.IdSubfamily.HasValue )
                    .OrderBy(f => f.Subfamilia)
                    .ToListAsync();

                var idsItems = items.Select(i => i.IdSubfamily.Value).ToList();

                var result = await _context.MaterialWithCounts
                    .Where(m => idsItems.Contains(m.IdSubfamilia) && m.Vigente == true && m.Id != idMasterFamily && !existentes.Contains(m.Id)  )
                    .OrderBy(m => m.Articulo)
                    .AsNoTracking()
                    .ToListAsync();

                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplies for company {idMasterFamily}", idMasterFamily);
                throw;
            }
        }


        public async Task<List<MasterFamilyDelison>> GetSuppliesMasterFamily(int idCompany)
        {
            try
            {
                var items = await _context.MasterFamilyDelison
                    .Where(m => m.IdCompany == idCompany && m.Active == true)
                    .OrderByDescending(m => m.Vigente)
                    .ToListAsync();                
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ");
                throw;
            }
        }

        public async Task<List<Catalog>> GetCatalog( int idCompany)
        {
            try
            { 
                // Obtener los IDs de master families existentes para la compañía
                var existentes = await _context.MasterFamilyDelison
                    .Where(m => m.IdCompany == idCompany && m.Active == true && m.MasterFamily != null)
                    .Select(m => m.MasterFamily.Value)
                    .ToListAsync();

                // Consultar catálogos tipo FAM-CAT que no estén ya registrados en MasterFamilyDelison
                var items = await _context.Catalogs
                    .Where(c => c.Active == 1 && c.Type == "FAM-CAT" && c.IdCompany == idCompany && c.Vigente == true  && c.Active == 1 && !existentes.Contains(c.Id))
                    .OrderByDescending(cat => cat.Vigente)
                    .ThenBy(cat => cat.Description)
                    .AsNoTracking()
                    .ToListAsync();

                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Catalogs");
                throw;
            }
        }

        public async Task<bool> SaveMasterFamily(Family family)
        {
            var data = new MasterFamilyDelison
            {
                IdCompany = family.IdCompany,
                MasterFamily = family.MasterFamily,
                Vigente = family.Vigente,
                Active = true
            };
        
            _context.MasterFamilyDelison.Add(data);
        
            var saved = await _context.SaveChangesAsync();
        
            return saved > 0;
        }

        public async Task<MasterFamilyDelison> UpdateMasterFamily(int id, Family family)
        {
            var existingItem = await _context.MasterFamilyDelison.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to update non-existent Supply with ID {Id}", id);
                return null;
            }

            try
            {
                existingItem.MasterFamily = family.MasterFamily;
                existingItem.Vigente = family.Vigente;
                await _context.SaveChangesAsync();
        
            return existingItem;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating Supply with ID {Id}", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Supply with ID {Id}", id);
                throw;
            }
        
            
        }
        public async Task<CatalogByMasterFamDelison> UpdateDetailMasterFamily(DetailFamily detailFamily)
        {
            // Buscar por los campos IdMaster e IdCatalog (la entidad tiene una clave simple 'Id')
            var existingItem = await _context.CatalogByMasterFamDelison
                .FirstOrDefaultAsync(c => c.IdMaster == detailFamily.IdMaster && c.IdCatalog == detailFamily.IdCatalog);

            if (existingItem == null)
            {
                var newItem = new CatalogByMasterFamDelison
                {
                    IdMaster = detailFamily.IdMaster,
                    IdCatalog = detailFamily.IdCatalog,
                    Vigente = detailFamily.Vigente,
                    Active = true
                };

                await _context.CatalogByMasterFamDelison.AddAsync(newItem);
                await _context.SaveChangesAsync();
                return newItem;
            }

            try
            {
                existingItem.Vigente = detailFamily.Vigente;
                await _context.SaveChangesAsync();

                return existingItem;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating Supply with ID {Id}", detailFamily.IdCatalog);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Supply with ID {Id}", detailFamily.IdCatalog);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            var existingItem = await _context.MasterFamilyDelison.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to delete non-existent Supply with ID {Id}", id);
                return false;
            }

            try
            {
                existingItem.Active = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Supply with ID {Id}", id);
                throw;
            }
        }

    }
    public interface IFamilySubFamilyDelisonService
    {
        Task<List<CatalogByMasterFamView>> GetDetailByMaster(int idCompany, int idMasterFamily);
        Task<List<MaterialWithCount>> GetArticulosSubFamilyByMaster( int idMasterFamily);
        Task<List<MaterialWithCount>> GetArticulosSubFamilyByMasterVigentes( int idCompany, int idMasterFamily,int idFamilia);
        Task<List<CatalogByMasterFamView>> GetSubFamilyByMaster( int idMasterFamily);
        Task<List<CatalogByMasterFamView>> GetSubFamilyByMasterVigentes( int idCompany, int idMasterFamily, int idFamilia);
        Task<List<FamilySubFamilyView>> GetSupplies(int idCompany);
        Task<List<MasterFamilyDelison>> GetSuppliesMasterFamily(int idCompany);
        Task<List<Catalog>> GetCatalog( int idCompany);
        Task<bool> SaveMasterFamily(Family family);
        Task<bool> Delete(int id);
        Task<MasterFamilyDelison> UpdateMasterFamily(int id, Family family);
        Task<CatalogByMasterFamDelison> UpdateDetailMasterFamily(DetailFamily detailFamily);
    }

     public class Family
        {
            public int IdCompany { get; set; }
            public int MasterFamily { get; set; }
            public bool Vigente { get; set; }
            
        }

    public class DetailFamily
        {
            public int IdCatalog { get; set; }
            public int IdMaster { get; set; }
            public bool Vigente { get; set; }
            
        }
// ...existing code...
}
