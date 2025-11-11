using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehouse.Models.Views;

namespace Warehouse.Service;

public class ProviderTypeService : IProviderTypeService
{
    private readonly DbWarehouseContext _context;
    private readonly ILogger<ProviderTypeService> _logger;

    public ProviderTypeService(DbWarehouseContext context, ILogger<ProviderTypeService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<ProviderType>> GetByProviderId(int idProvider)
    {
        try
        {
            _logger.LogInformation("Fetching provider types for provider {IdProvider}", idProvider);

            var providerTypes = await _context.ProviderTypes
                .Where(pt => pt.IdProvider == idProvider)
                .OrderBy(pt => pt.NameParent)
                .ThenBy(pt => pt.NameSubparent)
                .ThenBy(pt => pt.NameProduct)
                .ToListAsync();

            _logger.LogInformation("Found {Count} provider types for provider {IdProvider}", providerTypes.Count, idProvider);
            return providerTypes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching provider types for provider {IdProvider}", idProvider);
            throw;
        }
    }
}

public interface IProviderTypeService
{
    Task<List<ProviderType>> GetByProviderId(int idProvider);
}