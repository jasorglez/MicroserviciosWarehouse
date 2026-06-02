using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse.Service;

public interface ILoyaltyProgramService
{
    Task<List<LoyaltyProgram>> GetByProductAsync(int idCompany, int idProduct);
}

public class LoyaltyProgramService : ILoyaltyProgramService
{
    private readonly DbWarehouseContext _context;
    private readonly ILogger<LoyaltyProgramService> _logger;

    public LoyaltyProgramService(DbWarehouseContext context, ILogger<LoyaltyProgramService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<LoyaltyProgram>> GetByProductAsync(int idCompany, int idProduct)
    {
        return await _context.LoyaltyPrograms
            .Where(lp => lp.IdCompany == idCompany && lp.IdProduct == idProduct && lp.Active)
            .ToListAsync();
    }
}
