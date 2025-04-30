using Microsoft.EntityFrameworkCore;
using Warehouse.Models;
using Warehouse.Models.DTOs;

namespace Warehouse.Service;

public class PricesXProductsPresentationService : IPricesXProductsPresentationService
{
    private readonly DbWarehouseContext _context;
    private readonly ILogger<PricesXProductsPresentationService> _logger;

    public PricesXProductsPresentationService(DbWarehouseContext dbContext,
        ILogger<PricesXProductsPresentationService> logger)
    {
        _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        ;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    public async Task<IEnumerable<PricesXProductsPresentationsDTO>> GetPrices()
    {
        return await _context.PricesXProductsPresentation
            .Where(p => p.Active == true)
            .Include(p => p.Catalog)
            .Select(p => new PricesXProductsPresentationsDTO
            {
                Id = p.Id,
                IdMaterials = p.IdMaterials,
                IdCatalogs = p.IdCatalogs,
                Description = p.Description,
                Price = p.Price,
                Active = p.Active,
                Catalog = new CatalogDTO
                {
                    Id = p.Catalog.Id,
                    IdCompany = p.Catalog.IdCompany,
                    Description = p.Catalog.Description,
                    ValueAddition = p.Catalog.ValueAddition ?? "No disponible",
                    ValueAddition2 = p.Catalog.ValueAddition2 ?? "No disponible",
                    Type = p.Catalog.Type ?? string.Empty,
                    IdElection = p.Catalog.IdElection ?? true,
                    Active = p.Catalog.Active
                }
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<PricesXProductsPresentationsDTO> GetPricesById(int idPrice)
    {
        var priceProduct = await _context.PricesXProductsPresentation
            .Where(p => p.Id == idPrice && p.Active)
            .Include(p => p.Catalog)
            .FirstOrDefaultAsync();

        if (priceProduct == null)
        {
            return null;
        }

        return new PricesXProductsPresentationsDTO()
        {
            Id = priceProduct.Id,
            IdMaterials = priceProduct.IdMaterials,
            IdCatalogs = priceProduct.IdCatalogs,
            Description = priceProduct.Description,
            Price = priceProduct.Price,
            Active = priceProduct.Active,
            Catalog = new CatalogDTO
            {
                Id = priceProduct.Catalog.Id,
                IdCompany = priceProduct.Catalog.IdCompany,
                Description = priceProduct.Catalog.Description,
                ValueAddition = priceProduct.Catalog.ValueAddition,
                ValueAddition2 = priceProduct.Catalog.ValueAddition2,
                Type = priceProduct.Catalog.Type,
                IdElection = priceProduct.Catalog.IdElection,
                Active = priceProduct.Catalog.Active
            }
        };
    }

    public async Task Save(PricesXProductsPresentation priceProduct)
    {
        if (priceProduct.Price <= 0)
            throw new ArgumentException("El precio debe ser mayor a cero.");

        var materialExists = await _context.Materials.AnyAsync(m => m.Id == priceProduct.IdMaterials && m.Active);
        if (!materialExists)
            throw new ArgumentException("El material asociado no existe o está inactivo.");

        try
        {
            _context.PricesXProductsPresentation.Add(priceProduct);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database update error while saving PricesXProductsPresentation entity");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while saving PricesXProductsPresentation entity");
            throw;
        }
    }

    public async Task<bool> Update(int id, CreatePriceXProductsPresentationDTO dto)
    {
        var existing = await _context.PricesXProductsPresentation.FindAsync(id);
        if (existing == null)
            return false;

        existing.IdMaterials = dto.IdMaterials;
        existing.IdCatalogs = dto.IdCatalogs;
        existing.Description = dto.Description;
        existing.Price = dto.Price;
        existing.Active = dto.Active;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Delete(int idPrice)
    {
        var existing = await _context.PricesXProductsPresentation.FindAsync(idPrice);
        if (existing == null)
        {
            _logger.LogWarning("Attempted to disable non-existent PricesXProductsPresentation with ID {Id}", idPrice);
            return false;
        }

        try
        {
            existing.Active = false;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while disabling PricesXProductsPresentation with ID {Id}", idPrice);
            throw;
        }
    }
}

public interface IPricesXProductsPresentationService
{
    Task<IEnumerable<PricesXProductsPresentationsDTO>> GetPrices();
    Task<PricesXProductsPresentationsDTO> GetPricesById(int idPrice);
    Task Save(PricesXProductsPresentation pricesXProductsPresentation);
    Task<bool> Update(int id, CreatePriceXProductsPresentationDTO dto);
    Task<bool> Delete(int idPrice);
}