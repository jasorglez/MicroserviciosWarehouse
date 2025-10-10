using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehouse.Models;

namespace Warehouse.Service.Delison
{
    public class MaterialDelisonService : IMaterialDelisonService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<MaterialDelisonService> _logger;

        public MaterialDelisonService(DbWarehouseContext context, ILogger<MaterialDelisonService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<object>> GetSupplies(int idCompany, string type)
        {
            try
            {
                var materials = await _context.MaterialsDelison
                    .ToListAsync();

                return materials.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplies for company {IdCompany} and type {Type}", idCompany, type);
                throw;
            }
        }
    }
    public interface IMaterialDelisonService
    {
        Task<List<object>> GetSupplies(int idCompany, string type);
    }

}
