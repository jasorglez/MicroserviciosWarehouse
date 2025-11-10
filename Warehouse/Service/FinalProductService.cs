using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehouse.Models.Views;

namespace Warehouse.Service;

public class FinalProductService : IFinalProductService
{
    private readonly DbWarehouseContext _context;
    private readonly ILogger<FinalProductService> _logger;

    public FinalProductService(DbWarehouseContext context, ILogger<FinalProductService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<FinalProduct>> GetByCompanyId(int idCompany)
    {
        try
        {
            _logger.LogInformation("Fetching final products for company {IdCompany}", idCompany);

            var products = await _context.FinalProducts
                .Where(fp => fp.IdCompany == idCompany && fp.Active == 1)
                .OrderBy(fp => fp.Category)
                .ThenBy(fp => fp.Presentation)
                .ToListAsync();

            _logger.LogInformation("Found {Count} final products for company {IdCompany}", products.Count, idCompany);
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching final products for company {IdCompany}", idCompany);
            throw;
        }
    }
}

public interface IFinalProductService
{
    Task<List<FinalProduct>> GetByCompanyId(int idCompany);
}