﻿using Microsoft.EntityFrameworkCore;
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

        public async Task<List<Catalog>> Get(string type)
        {
            try
            {
                return await _context.Catalogs
                    .Where(c => c.Active == 1 && c.Type==type)
                    .Select(cat => new Catalog
                    {
                        Id = cat.Id,
                        IdCompany = cat.IdCompany,
                        Description = cat.Description,
                        Type          = cat.Type,
                        ValueAddition =cat.ValueAddition,
                        ParentId = cat.ParentId
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
        
        public async Task<List<Catalog>> NewGet(int idCompany, string type)
        {
            try
            {
                return await _context.Catalogs
                    .Where(c => c.Active == 1 && c.IdCompany == idCompany && c.Type==type)
                    .Select(cat => new Catalog
                    {
                        Id = cat.Id,
                        IdCompany = cat.IdCompany,
                        Description = cat.Description,
                        Type          = cat.Type,
                        ValueAddition =cat.ValueAddition,
                        ParentId = cat.ParentId
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


    }

    public interface ICatalogService
    {
        Task<List<Catalog>> Get(string type);
        Task<List<Catalog>> NewGet(int idCompany, string type);
        Task<List<object>> GetFamilyCatalogs(int idCompany);

        Task<List<object>> GetSubfamilyCatalogs(int parentId);
        Task Save(Catalog cat);
        Task<bool> Delete(int id);
    }
}
