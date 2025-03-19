using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Contracts;
using Warehouse.Models;

namespace Warehouse.Service
{
    public class CatalogService : ICatalogService
    {        
        private readonly DbWarehouseContext _context;
        private readonly ILogger<CatalogService> _logger;

        public CatalogService(DbWarehouseContext dbContext, ILogger<CatalogService> logger)
        {
            _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<Catalog>> GetType(string type, int idCompany)
        {
            try
            {
                return await _context.Catalogs
                    .Where(c => c.Active ==1 && c.Type==type && c.IdCompany==idCompany)
                    .Select(cat => new Catalog
                    {
                        Id             = cat.Id,
                        IdCompany      = cat.IdCompany,
                        Description    = cat.Description,
                        ValueAddition  = cat.ValueAddition,
                        ValueAddition2 = cat.ValueAddition2,
                        Picture        = cat.Picture,
                        Type           = cat.Type,
                        ParentId       = cat.ParentId,
                        IdElection     = cat.IdElection,
                        Active         = cat.Active
                    })
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Catalogs");
                throw;
            }
        }

        public async Task<List<object>> GetFamilyCatalogs(int idCompany)
        {
            try
            {
                return await _context.Catalogs
                    .Where(c => c.Active == 1 && c.Type == "family" && c.IdCompany == idCompany)
                    .Select(c => new
                    {
                        c.Id,
                        c.IdCompany,
                        c.Description,
                        c.ParentId,
                        c.Type,
                        c.Active
                    })
                    .AsNoTracking()
                    .ToListAsync<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving family catalogs");
                throw;
            }
        }
        
        public async Task<List<object>> GetSubfamilyCatalogs(int parentId)
        {
            try
            {
                return await _context.Catalogs
                    .Where(c => c.Active == 1 && c.Type == "subfamily" && c.ParentId == parentId)
                    .Select(c => new
                    {
                        c.Id,
                        c.IdCompany,
                        c.Description,
                        c.ParentId,
                        c.Type,
                        c.Active
                    })
                    .AsNoTracking()
                    .ToListAsync<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subfamily catalogs");
                throw;
            }
        }

        public async Task Save(Catalog cat)
        {
            try
            {
                _context.Catalogs.Add(cat);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update error while saving Catalogs");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Catalogs");
                throw;
            }
        }

        public async Task<bool> Update(int id, Catalog cat)
        {
            var existingConfig = await _context.Catalogs.FindAsync(id);
            if (existingConfig == null)
            {
                return false;
            }

            try
            {
                _context.Entry(existingConfig).CurrentValues.SetValues(cat);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating configuration with ID {Id}.", id);
                return false;
            }
        }

        public async Task<bool> Delete(int id)
        {
            var existingCt = await _context.Catalogs.FindAsync(id);
            if (existingCt == null)
            {
                _logger.LogWarning("Attempted to update non-existent Catalogs With ID {Id}", id);
                return false;
            }
            try
            {
                existingCt.Active = 0;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error occurred while updating Catalogs", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating Catalog", id);
                throw;
            }
        }

        public async Task<List<ProcessXPermission>> GetProcessXPermissions(int idProcces)
        { //aqui paso soriano
            try
            {
                var query = _context.ProcessXPermissions
                    .Where(p => p.IdProcces == idProcces)
                    .AsNoTracking();           

                return await query
                    .Select(p => new ProcessXPermission
                    {
                        Id = p.Id,
                        IdProcces   = p.IdProcces,
                        Description = p.Description,
                        Select      = p.Select,
                        Active      = p.Active
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ProcessXPermissions");
                throw;
            }
        }

        public async Task Savexpermission(ProcessXPermission perm)
        {
            try
            {
                _context.ProcessXPermissions.Add(perm);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update error while saving Permission");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Permissions");
                throw;
            }
        }

        public async Task<bool> UpdatexPermission(int id, ProcessXPermission perm)
        {
            try
            {
                var existingPermission = await _context.ProcessXPermissions.FindAsync(id);

                if (existingPermission == null)
                {
                    return false; // Entity not found
                }

                // Update the properties (excluding Id)

                existingPermission.IdProcces = perm.IdProcces;
                existingPermission.Description = perm.Description;
                existingPermission.Select = perm.Select;                            
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating configuration with ID {Id}.", id);
                return false;
            }
        }

    }

    public interface ICatalogService
    {
        Task<List<Catalog>> GetType(string type, int idCompany);
        Task<List<object>> GetFamilyCatalogs(int idCompany);
        Task<List<object>> GetSubfamilyCatalogs(int parentId);
        Task Save(Catalog cat);
        Task<bool> Update(int id, Catalog cat);
        Task<bool> Delete(int id);
        Task<List<ProcessXPermission>> GetProcessXPermissions(int idProcces);
        Task<bool> UpdatexPermission(int id, ProcessXPermission perm);
        Task Savexpermission(ProcessXPermission perm);
    }
}
