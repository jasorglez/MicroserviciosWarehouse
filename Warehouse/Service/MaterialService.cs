﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehouse.Models;
using Warehouse.Models.Views;


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

        public async Task<List<ProveedoresxtypeView>> MaterialsxProvView(int idCompany)
        {
            try
            {
                var result = await _context.ProveedoresxtypeViews
                    .Where(m => m.IdRoot == idCompany)
                    .OrderBy(m => m.Company)
                    .AsNoTracking()
                    .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Materials view");
                throw;
            }
        }


        public async Task<List<MaterialWithCount>> GetMaterialsWithCounts(int idCompany)
        {
            try
            {
                var result = await _context.MaterialWithCounts
                    .Where(m => m.IdCompany == idCompany)
                    .OrderBy(m => m.Articulo)
                    .AsNoTracking()
                    .ToListAsync();

                // Aplicar valores por defecto después de cargar desde BD
                foreach (var item in result)
                {
                    item.Insumo     = string.IsNullOrEmpty(item.Insumo) ? "N/A" : item.Insumo;
                    item.Articulo   = string.IsNullOrEmpty(item.Articulo) ? "Sin especificar" : item.Articulo;
                    item.Categoria  = string.IsNullOrEmpty(item.Categoria) ? "Sin categoría" : item.Categoria;
                    item.Familia    = string.IsNullOrEmpty(item.Familia) ? "Sin familia" : item.Familia;
                    item.Subfamilia = string.IsNullOrEmpty(item.Subfamilia) ? "Sin subfamilia" : item.Subfamilia;
                    item.Picture    = string.IsNullOrEmpty(item.Picture) ? "sin-imagen.jpg" : item.Picture;
                    item.ProviderCount ??= 0;
                    item.SubfamilyCount ??= 0;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Materials with counts for company {IdCompany}", idCompany);
                throw;
            }
        }

        public async Task<List<MaterialsXProvider>> MaterialsxProv(string Typereference, int idProv)
        {
            try
            {
                var result = await _context.MaterialsxProviders
                    .Where(m => m.IdProvider==idProv)
                    .OrderBy(m => m.TypeReference==Typereference)
                    .AsNoTracking()
                    .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Materials view");
                throw;
            }
        }


        public async Task<List<MaterialsxProvExist>> GetMaterialsView(int idCompany)
        {
            try
            {
                var result = await _context.MaterialsxProvExists
                    .Where(m => m.IdCompany == idCompany)
                    .OrderBy(m => m.Description)
                    .AsNoTracking()
                    .ToListAsync();

                // Aplicar valores por defecto después de cargar desde BD
                foreach (var item in result)
                {
                    // Strings - aplicar valores por defecto si son null o vacíos
                    item.TypeOcorReq = string.IsNullOrEmpty(item.TypeOcorReq) ? "N/A" : item.TypeOcorReq;
                    item.Insumo = string.IsNullOrEmpty(item.Insumo) ? "N/A" : item.Insumo;
                    item.Barcode = string.IsNullOrEmpty(item.Barcode) ? "N/A" : item.Barcode;
                    item.Company = string.IsNullOrEmpty(item.Company) ? "N/A" : item.Company;
                    item.Articulo = string.IsNullOrEmpty(item.Articulo) ? "Sin especificar" : item.Articulo;
                    item.Description = string.IsNullOrEmpty(item.Description) ? "Sin descripción" : item.Description;
                    item.Folio = string.IsNullOrEmpty(item.Folio) ? "N/A" : item.Folio;
                    item.Picture = string.IsNullOrEmpty(item.Picture) ? "sin-imagen.jpg" : item.Picture;
                    item.DescriptionPackage = string.IsNullOrEmpty(item.DescriptionPackage) ? "N/A" : item.DescriptionPackage;
                    item.Measure = string.IsNullOrEmpty(item.Measure) ? "Unidad" : item.Measure;
                    item.FolioOcorReq = string.IsNullOrEmpty(item.FolioOcorReq) ? "N/A" : item.FolioOcorReq;

                    // Decimales - asignar 0 si son null
                    item.Price ??= 0;
                    item.Quantity ??= 0;
                    item.CostoMN ??= 0;
                    item.CostoDLL ??= 0;
                    item.VentaMN ??= 0;
                    item.VentaDLL ??= 0;
                    item.PackageQuantity ??= 1;
                    item.WeightOrVolumes ??= 0;
                    item.InOrOutQuantity ??= 0;
                    item.Pending ??= 0;

                    // Enteros - asignar 0 si son null
                    item.IdBranch ??= 0;
                    item.IdCustomer ??= 0;
                    item.IdCategory ??= 0;
                    item.IdFamilia ??= 0;
                    item.IdSubfamilia ??= 0;
                    item.IdMedida ??= 0;
                    item.IdUbication ??= 0;
                    item.StockMin ??= 0;
                    item.StockMax ??= 0;
                    item.Expiration ??= 0;

                    // Booleans - asignar false si son null
                    item.AplicaResg ??= false;
                    item.Vigente ??= true; // Este probablemente debería ser true por defecto

                    // DateTime - puedes asignar fecha actual o una fecha por defecto
                    // item.Date ??= DateTime.Now; // Solo si lo necesitas
                    // item.FechaOc ??= DateTime.Now; // Solo si lo necesitas
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Materials view");
                throw;
            }
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
                        s.IdCategory,
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
                        s.StockMin, s.StockMax, s.Picture,
                        s.Vigente,  s.TypeMaterial,
                        s.DescriptionPackage,
                        s.PackageQuantity, s.Measure,
                        s.WeightOrVolumes,s.Expiration,

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
                // Crear una nueva instancia con los campos necesarios
                var newMaterial = new Material
                {
                    // Asignar campos del material recibido
                    IdCompany = material.IdCompany,
                    IdBranch = material.IdBranch,
                    IdCustomer = material.IdCustomer,
                    Insumo = material.Insumo,
                    Articulo = material.Articulo,
                    BarCode = material.BarCode,
                    IdCategory = material.IdCategory,
                    IdFamilia = material.IdFamilia,
                    IdSubfamilia = material.IdSubfamilia,
                    IdMedida = material.IdMedida,
                    IdUbication = material.IdUbication,
                    Description = material.Description,
                    Date = material.Date ?? DateTime.UtcNow, // Valor por defecto si es null
                    AplicaResg = material.AplicaResg,
                    CostoMN = material.CostoMN,
                    CostoDLL = material.CostoDLL,
                    VentaMN = material.VentaMN,
                    VentaDLL = material.VentaDLL,
                    StockMin = material.StockMin,
                    StockMax = material.StockMax,
                    Picture = material.Picture,
                    DescriptionPackage = material.DescriptionPackage,
                    PackageQuantity = material.PackageQuantity,
                    Measure = material.Measure,
                    WeightOrVolumes = material.WeightOrVolumes,
                    Expiration = material.Expiration,
                    TypeMaterial = material.TypeMaterial,
                    Vigente = material.Vigente ?? true, // Valor por defecto si es null
                    Active = material.Active ?? true // Valor por defecto si es null
                };

                _context.Materials.Add(newMaterial);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Material");
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
                existingItem.IdCategory = material.IdCategory;
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
                existingItem.DescriptionPackage = material.DescriptionPackage;
                existingItem.PackageQuantity = material.PackageQuantity;
                existingItem.Measure = material.Measure;
                existingItem.WeightOrVolumes = material.WeightOrVolumes;
                existingItem.Expiration = material.Expiration;                
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
        Task<List<ProveedoresxtypeView>> MaterialsxProvView(int idCompany);
        Task<List<MaterialWithCount>> GetMaterialsWithCounts(int idCompany);
        Task<List<MaterialsXProvider>> MaterialsxProv(string Typereference, int idProv);
        Task<List<object>> GetSupplies(int idCompany, string typematerial);
        Task<List<object>> Get2Supplies(int idCompany);
        Task<List<MaterialsxProvExist>> GetMaterialsView(int idCompany);
        Task<List<object>> GetSuppliesByNameOrBarCode(int idCompany, string nameOrBarCode);
        Task Save(Material material);
        Task<Material?> Update(int id, Material material);
        Task<bool> Delete(int id);
    }

}

    