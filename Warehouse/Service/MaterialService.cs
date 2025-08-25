using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehouse.Models;


namespace Warehouse.Service
{
    public class MaterialService : IMaterialService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<MaterialService> _logger;

        public MaterialService(DbWarehouseContext context, ILogger<MaterialService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<object>> GetSupplies(int idCompany, string typematerial)
        {
            try
            {
                return await _context.Materials
                    .Where(s => s.Active == true && s.IdCompany == idCompany && s.TypeMaterial == typematerial)         
                    .Select(s => new
                    {
                        s.Id,                        
                        s.IdCompany,
                        s.IdBranch,
                        s.IdCustomer,
                        s.Insumo,
                        s.BarCode,
                        s.Articulo,
                        s.IdFamilia,
                        s.IdSubfamilia,
                        s.IdMedida,
                        s.IdUbication,
                        s.Description,
                        s.Date,
                        s.AplicaResg,
                        s.CostoMN,
                        s.CostoDLL,
                        s.VentaMN,
                        s.VentaDLL,
                        s.StockMin,
                        s.StockMax,
                        s.Picture,
                        s.Vigente,
                        s.TypeMaterial,
                        s.Active
                       
                    })
                    .OrderByDescending(s => s.Vigente)
                    .ThenBy(s => s.Description)
                    .AsNoTracking()
                    .ToListAsync<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Supplies");
                throw;
            }
        }

        public async Task<List<object>> Get2Supplies(int idCompany)
        {
            try
            {
                return await _context.Materials
                    .Where(s => s.Active == true && idCompany == s.IdCompany)
                    //.Include(s => s.PricesWithMaterial)
                    .Select(s => new
                    {
                        s.Id,                        
                        s.Description,s.Vigente,
                        s.Active,
                        /*PricePresentations = s.PricesWithMaterial.Select(s => new
                        {
                        s.Id,
                        s.IdCatalogs,
                        s.Description,
                        s.Price,
                        s.Active
                    }).ToList()*/
                    }).OrderByDescending(s => s.Vigente)
                      .ThenBy(s => s.Description)
                    .AsNoTracking()
                    .ToListAsync<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Supplies");
                throw;
            }
        }

        public async Task<List<object>> GetSuppliesByNameOrBarCode(int idCompany, string nameOrBarCode)
        {
            try
            {
                return await _context.Materials
                    .Where(s => s.Active == true && idCompany == s.IdCompany && 
                    (s.Insumo.Contains(nameOrBarCode) || s.BarCode.Contains(nameOrBarCode) || s.Description.Contains(nameOrBarCode)))
                 //   .Include(s => s.PricesWithMaterial)
                    .Select(s => new
                    {
                        s.Id,
                        s.BarCode,
                        s.Insumo,
                        s.Description,
                        s.Active,
                    /*    PricePresentations = s.PricesWithMaterial.Select(s => new
                        {
                            s.Id,
                            s.IdCatalogs,
                            s.Description,
                            s.Price,
                            s.Active
                        }).ToList()*/
                    }).OrderBy(s => s.Description)
                    .AsNoTracking()
                    .ToListAsync<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Supplies");
                throw;
            }
        }

        public async Task Save(Material material)
        {
            try
            {
                _context.Materials.Add(material);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Supply");
                throw;
            }
        }

        public async Task<Material?> Update(int id, Material material)
        {
            var existingItem = await _context.Materials.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to update non-existent Supply with ID {Id}", id);
                return null;
            }

            try
            {
                // Preserve the original ID and update only the necessary fields
                existingItem.IdCompany = material.IdCompany;
                existingItem.IdBranch = material.IdBranch;
                existingItem.IdCustomer = material.IdCustomer;
                existingItem.Insumo = material.Insumo;
                existingItem.Articulo = material.Articulo;
                existingItem.BarCode = material.BarCode;
                existingItem.IdFamilia = material.IdFamilia;
                existingItem.IdSubfamilia = material.IdSubfamilia;
                existingItem.IdMedida = material.IdMedida;
                existingItem.IdUbication = material.IdUbication;
                existingItem.Description = material.Description;
                existingItem.Date = material.Date;
                existingItem.AplicaResg = material.AplicaResg;
                existingItem.CostoMN = material.CostoMN;
                existingItem.CostoDLL = material.CostoDLL;
                existingItem.VentaMN = material.VentaMN;
                existingItem.VentaDLL = material.VentaDLL;
                existingItem.StockMin = material.StockMin;
                existingItem.StockMax = material.StockMax;
                existingItem.Picture = material.Picture;
                existingItem.TypeMaterial = material.TypeMaterial;
                existingItem.Vigente = material.Vigente;
                existingItem.Active = material.Active;
                
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

        public async Task<bool> Delete(int id)
        {
            var existingItem = await _context.Materials.FindAsync(id);
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

    public interface IMaterialService
    {
        Task<List<object>> GetSupplies(int idCompany, string typematerial);
        Task<List<object>> Get2Supplies(int idCompany);
        Task<List<object>> GetSuppliesByNameOrBarCode(int idCompany, string nameOrBarCode);
        Task Save(Material material);
        Task<Material?> Update(int id, Material material);
        Task<bool> Delete(int id);
    }

}