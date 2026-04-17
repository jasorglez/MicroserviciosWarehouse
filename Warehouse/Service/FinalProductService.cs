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

            // Primero obtenemos los productos desde la BD (aquí sí usamos await)
            var productsFromDb = await _context.FinalProducts
                .Where(fp => fp.IdCompany == idCompany && fp.Active == 1)
                .ToListAsync();

            // Luego ordenamos en memoria (sin await)
            var products = productsFromDb
                .OrderBy(fp => fp.Category)
                .ThenBy(fp => ExtraerNumero(fp.Presentation))
                .ToList();


            _logger.LogInformation("Found {Count} final products for company {IdCompany}", products.Count, idCompany);

            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching final products for company {IdCompany}", idCompany);
            throw;
        }
    }

    int ExtraerNumero(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return 0;

        // extrae solo los dígitos
        var numeros = new string(texto.Where(char.IsDigit).ToArray());

        if (int.TryParse(numeros, out int valor))
            return valor;

        return 0; // si no hay números válidos
    }



}

public interface IFinalProductService
{
    Task<List<FinalProduct>> GetByCompanyId(int idCompany);
}