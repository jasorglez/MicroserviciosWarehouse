using Microsoft.EntityFrameworkCore;
using Warehouse.Models;
using Warehouse.Models.Views;

namespace Warehouse.Service;

public class InventarioService : IInventarioService
{
    private readonly DbWarehouseContext _context;
    private readonly ILogger<InventarioService> _logger;

    public InventarioService(DbWarehouseContext context, ILogger<InventarioService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger  = logger  ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<VwInventarioTotal>> ObtenerInventarioTotalAsync()
        => await _context.VwInventarioTotal.OrderBy(v => v.Insumo).ToListAsync();

    public async Task<IEnumerable<VwInventarioTotal>> ObtenerInventarioFiltradoAsync(int idCompany)
        => await _context.VwInventarioTotal
            .Where(v => v.IdCompany == idCompany)
            .OrderBy(v => v.Insumo)
            .ToListAsync();

    public async Task<object> ObtenerResumenAsync()
    {
        var inv = await _context.VwInventarioTotal.ToListAsync();
        return new
        {
            TotalProductos    = inv.Count,
            ValorTotal        = inv.Sum(v => v.Total),
            ProductosCriticos = inv.Count(v => v.EstadoStock == "CRÍTICO"),
            ProductosBajos    = inv.Count(v => v.EstadoStock == "BAJO"),
            ProductosOptimos  = inv.Count(v => v.EstadoStock == "ÓPTIMO"),
            ProductosAltos    = inv.Count(v => v.EstadoStock == "ALTO")
        };
    }

    public async Task<object> AjustarInventarioAsync(AjusteInventarioDto dto)
    {
        var item = await _context.VwInventarioTotal
            .FirstOrDefaultAsync(v => v.Id == dto.IdMaterial && v.IdCompany == dto.IdCompany);

        decimal existenciaActual = item?.Existencia ?? 0;
        decimal delta            = dto.CantidadFisica - existenciaActual;

        if (delta == 0)
            return new { message = "Sin cambios, la existencia ya coincide", delta = 0, idInandout = (int?)null };

        string  tipo     = delta > 0 ? "IN" : "OUT";
        decimal cantidad = Math.Abs(delta);

        var mov = new Inandout
        {
            Folio        = $"AJU-{DateTime.Today:yyyyMMdd}-{dto.IdMaterial}",
            Date         = DateTime.Today,
            DeliveryDate = DateTime.Today,
            Type         = tipo,
            IdWarehouse  = dto.IdWarehouse,
            DeliverName  = "AJUSTE FÍSICO",
            NumBill      = "AJUSTE",
            Comment      = !string.IsNullOrWhiteSpace(dto.Comentario)
                               ? dto.Comentario
                               : "Ajuste de inventario físico",
            CountRow = 1,
            Active   = true
        };
        _context.Inandouts.Add(mov);
        await _context.SaveChangesAsync();

        _context.Detailsinandout.Add(new Detailsinandout
        {
            IdInandout = mov.Id,
            IdProduct  = dto.IdMaterial,
            Quantity   = cantidad,
            Pending    = 0,
            Total      = cantidad,
            Active     = true
        });
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Ajuste inventario: material={IdMaterial} almacén={IdWarehouse} delta={Delta} tipo={Tipo}",
            dto.IdMaterial, dto.IdWarehouse, delta, tipo);

        return new { message = "Ajuste realizado correctamente", delta, tipo, idInandout = mov.Id };
    }
}

public interface IInventarioService
{
    Task<IEnumerable<VwInventarioTotal>> ObtenerInventarioTotalAsync();
    Task<IEnumerable<VwInventarioTotal>> ObtenerInventarioFiltradoAsync(int idCompany);
    Task<object> ObtenerResumenAsync();
    Task<object> AjustarInventarioAsync(AjusteInventarioDto dto);
}
