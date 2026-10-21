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
        if (item == null)
            throw new InvalidOperationException("El material no pertenece a la empresa seleccionada.");

        var configuredBrands = await (from relation in _context.MaterialBrands.AsNoTracking()
                                      join catalog in _context.Catalogs.AsNoTracking() on relation.IdCatalog equals catalog.Id
                                      where relation.IdMaterial == dto.IdMaterial && relation.Active
                                          && catalog.IdCompany == dto.IdCompany && catalog.Type == "BRAND" && catalog.Active == 1
                                      orderby catalog.Description
                                      select new { relation.IdCatalog, catalog.Description })
            .ToListAsync();

        int? selectedBrandId = null;
        string? selectedBrandName = null;
        decimal existenciaActual;
        if (dto.IdBrand.HasValue)
        {
            if (dto.IdBrand.Value > 0)
            {
                var selectedBrand = configuredBrands.FirstOrDefault(brand => brand.IdCatalog == dto.IdBrand.Value);
                if (selectedBrand == null)
                    throw new InvalidOperationException("La marca seleccionada no está configurada para este material.");
                selectedBrandId = selectedBrand.IdCatalog;
                selectedBrandName = selectedBrand.Description;
            }

            // En materiales de una sola marca se conserva como saldo de esa marca
            // el inventario histórico que todavía no tenía marca capturada.
            existenciaActual = configuredBrands.Count == 1
                ? await GetMaterialStockAsync(dto.IdMaterial, dto.IdWarehouse, null, allBrands: true)
                : await GetMaterialStockAsync(dto.IdMaterial, dto.IdWarehouse, dto.IdBrand.Value == 0 ? null : dto.IdBrand);
        }
        else
        {
            existenciaActual = await GetMaterialStockAsync(dto.IdMaterial, dto.IdWarehouse, null, allBrands: true);
        }
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
            IdBrand    = selectedBrandId,
            BrandName  = selectedBrandName ?? (dto.IdBrand == 0 ? "Sin marca" : null),
            Active     = true
        });
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Ajuste inventario: material={IdMaterial} almacén={IdWarehouse} delta={Delta} tipo={Tipo}",
            dto.IdMaterial, dto.IdWarehouse, delta, tipo);

        return new { message = "Ajuste realizado correctamente", delta, tipo, idInandout = mov.Id };
    }

    private async Task<decimal> GetMaterialStockAsync(int idMaterial, int idWarehouse, int? idBrand, bool allBrands = false)
    {
        var query = _context.Detailsinandout.AsNoTracking()
            .Where(detail => detail.Active && detail.IdProduct == idMaterial);
        if (!allBrands)
        {
            if (idBrand.HasValue) query = query.Where(detail => detail.IdBrand == idBrand.Value);
            else query = query.Where(detail => detail.IdBrand == null);
        }

        return await query
            .Join(_context.Inandouts.AsNoTracking(), detail => detail.IdInandout, movement => movement.Id,
                (detail, movement) => new { detail, movement })
            .Where(row => row.movement.Active != false && row.movement.IdWarehouse == idWarehouse
                && (row.movement.Type == "IN" || row.movement.Type == "OUT"))
            .SumAsync(row => (decimal?)(row.movement.Type == "IN" ? row.detail.Quantity : -row.detail.Quantity)) ?? 0m;
    }
}

public interface IInventarioService
{
    Task<IEnumerable<VwInventarioTotal>> ObtenerInventarioTotalAsync();
    Task<IEnumerable<VwInventarioTotal>> ObtenerInventarioFiltradoAsync(int idCompany);
    Task<object> ObtenerResumenAsync();
    Task<object> AjustarInventarioAsync(AjusteInventarioDto dto);
}
