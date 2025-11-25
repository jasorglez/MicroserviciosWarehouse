using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehouse.Models.Views;

namespace Warehouse.Service;


public class InventarioService : IInventarioService
{
    private readonly DbWarehouseContext _context;
    private readonly ILogger<InventarioService> _logger;

    public InventarioService(DbWarehouseContext context, ILogger<InventarioService> logger)
    {

        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<VwInventarioTotal>> ObtenerInventarioTotalAsync()
    {
        return await _context.VwInventarioTotal
            .OrderBy(v => v.Insumo)
            .ToListAsync();
    }

    public async Task<IEnumerable<VwInventarioTotal>> ObtenerInventarioFiltradoAsync(int idCompany)
    {
        return await _context.VwInventarioTotal
            .Where(v => v.IdCompany == idCompany)
            .OrderBy(v => v.Insumo)
            .ToListAsync();
    }

    public async Task<object> ObtenerResumenAsync()
    {
        var inventario = await _context.VwInventarioTotal.ToListAsync();

        return new
        {
            TotalProductos = inventario.Count,
            ValorTotal = inventario.Sum(v => v.Total),
            ProductosCriticos = inventario.Count(v => v.EstadoStock == "CRÍTICO"),
            ProductosBajos = inventario.Count(v => v.EstadoStock == "BAJO"),
            ProductosOptimos = inventario.Count(v => v.EstadoStock == "ÓPTIMO"),
            ProductosAltos = inventario.Count(v => v.EstadoStock == "ALTO")
        };
    }
}


 
  public interface IInventarioService
    {
        Task<IEnumerable<VwInventarioTotal>> ObtenerInventarioTotalAsync();
        Task<IEnumerable<VwInventarioTotal>> ObtenerInventarioFiltradoAsync(int idCompany);
        Task<object> ObtenerResumenAsync();
    }

