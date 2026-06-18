using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;

namespace Warehouse.Service.Delison
{
    // Desglose de compras por proveedor (auditoría del promedio ponderado).
    public class ProveedorCostoDto
    {
        public string Proveedor { get; set; } = "";
        public decimal Cantidad { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal PrecioUnitario { get; set; }
    }

    // Costos calculados de un material básico sobre la ventana móvil.
    public class CostoPonderadoDto
    {
        public int IdMaterial { get; set; }
        public decimal PromedioPonderado { get; set; }   // Σcosto / Σcantidad
        public decimal UltimaCompra { get; set; }        // precio unitario de la recepción más reciente
        public decimal Maximo { get; set; }              // precio unitario más caro del periodo
        public decimal CantidadTotal { get; set; }
        public decimal CostoTotal { get; set; }
        public int VentanaMeses { get; set; }
        public string PeriodoInicio { get; set; } = "";  // yyyy-MM-dd
        public string PeriodoFin { get; set; } = "";
        public bool Parcial { get; set; }                // historial menor que la ventana (aún no representativo)
        public bool SinDatos { get; set; }               // no hay compras pagadas en la ventana
        public List<ProveedorCostoDto> Desglose { get; set; } = new();
    }

    public interface ICostosPonderadosService
    {
        Task<int> GetVentanaMeses(int idCompany);
        Task<int> SetVentanaMeses(int idCompany, int meses);
        Task<CostoPonderadoDto> GetByMaterial(int idCompany, int idMaterial, int? ventanaMeses);
        Task<List<CostoPonderadoDto>> GetBatch(int idCompany, List<int> idMaterials, int? ventanaMeses);
    }

    public class CostosPonderadosService : ICostosPonderadosService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<CostosPonderadosService> _logger;

        public CostosPonderadosService(DbWarehouseContext context, ILogger<CostosPonderadosService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        public async Task<int> GetVentanaMeses(int idCompany)
        {
            var cfg = await _context.CostosConfig.AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCompany == idCompany);
            return cfg?.VentanaMeses ?? 12;
        }

        public async Task<int> SetVentanaMeses(int idCompany, int meses)
        {
            if (meses < 1) meses = 1;
            var cfg = await _context.CostosConfig.FirstOrDefaultAsync(c => c.IdCompany == idCompany);
            if (cfg == null)
            {
                cfg = new CostosConfigDelison { IdCompany = idCompany, VentanaMeses = meses, DateModified = DateTime.UtcNow };
                _context.CostosConfig.Add(cfg);
            }
            else
            {
                cfg.VentanaMeses = meses;
                cfg.DateModified = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
            return meses;
        }

        public async Task<CostoPonderadoDto> GetByMaterial(int idCompany, int idMaterial, int? ventanaMeses)
        {
            var list = await GetBatch(idCompany, new List<int> { idMaterial }, ventanaMeses);
            return list.FirstOrDefault() ?? EmptyDto(idMaterial, ventanaMeses ?? await GetVentanaMeses(idCompany), DateOnly.FromDateTime(DateTime.Today));
        }

        public async Task<List<CostoPonderadoDto>> GetBatch(int idCompany, List<int> idMaterials, int? ventanaMeses)
        {
            var resultado = new List<CostoPonderadoDto>();
            idMaterials = idMaterials?.Where(i => i > 0).Distinct().ToList() ?? new List<int>();
            if (idMaterials.Count == 0) return resultado;

            int ventana = ventanaMeses ?? await GetVentanaMeses(idCompany);
            if (ventana < 1) ventana = 12;
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var fechaInicio = hoy.AddMonths(-ventana);

            // Recepciones pagadas en la ventana, con cantidad y costo válidos.
            // Costo = monto_mxn (Fase 4) o, si viene NULL, el pago en moneda original.
            var entradas = await _context.EntradasMolienda
                .Where(e => e.IdMaterial.HasValue && idMaterials.Contains(e.IdMaterial.Value)
                         && e.Active && e.Liberacion
                         && e.CantidadEntrada.HasValue && e.CantidadEntrada.Value > 0
                         && (e.MontoMxn ?? e.Pago) != null
                         && e.FechaRecepcion.HasValue && e.FechaRecepcion.Value >= fechaInicio)
                .Select(e => new
                {
                    IdMaterial = e.IdMaterial!.Value,
                    e.IdOc,
                    Cantidad = e.CantidadEntrada!.Value,
                    Costo = (e.MontoMxn ?? e.Pago) ?? 0m,
                    Fecha = e.FechaRecepcion!.Value
                })
                .ToListAsync();

            // Proveedor de cada recepción: entrada.IdOc == ocandreq.Id == detailsreqoc.IdMovement,
            // y el material es detailsreqoc.IdSupplie. NameProvider/ProvInt traen el nombre.
            var ocIds = entradas.Select(e => e.IdOc).Distinct().ToList();
            var provMap = new Dictionary<(int oc, int mat), string>();
            if (ocIds.Count > 0)
            {
                var dets = await _context.Detailsreqoc
                    .Where(d => ocIds.Contains(d.IdMovement) && idMaterials.Contains(d.IdSupplie) && d.Active == true)
                    .Select(d => new { d.IdMovement, d.IdSupplie, d.NameProvider, d.ProvInt })
                    .ToListAsync();
                foreach (var d in dets)
                {
                    var key = (d.IdMovement, d.IdSupplie);
                    if (!provMap.ContainsKey(key))
                        provMap[key] = (string.IsNullOrWhiteSpace(d.NameProvider) ? d.ProvInt : d.NameProvider) ?? "SIN PROVEEDOR";
                }
            }

            // Fecha de la compra pagada más antigua (de cualquier periodo) por material → para marcar "parcial".
            var oldest = await _context.EntradasMolienda
                .Where(e => e.IdMaterial.HasValue && idMaterials.Contains(e.IdMaterial.Value)
                         && e.Active && e.Liberacion
                         && e.CantidadEntrada.HasValue && e.CantidadEntrada.Value > 0
                         && (e.MontoMxn ?? e.Pago) != null
                         && e.FechaRecepcion.HasValue)
                .GroupBy(e => e.IdMaterial!.Value)
                .Select(g => new { Id = g.Key, Min = g.Min(x => x.FechaRecepcion) })
                .ToListAsync();
            var oldestByMat = oldest.ToDictionary(x => x.Id, x => x.Min);

            foreach (var idMat in idMaterials)
            {
                var rows = entradas.Where(e => e.IdMaterial == idMat).ToList();
                if (rows.Count == 0)
                {
                    var empty = EmptyDto(idMat, ventana, fechaInicio);
                    // Parcial si ni siquiera hay historial previo a la ventana.
                    empty.Parcial = !oldestByMat.TryGetValue(idMat, out var min0) || (min0.HasValue && min0.Value > fechaInicio);
                    resultado.Add(empty);
                    continue;
                }

                decimal cantTotal = rows.Sum(r => r.Cantidad);
                decimal costoTotal = rows.Sum(r => r.Costo);
                decimal promedio = cantTotal > 0 ? Math.Round(costoTotal / cantTotal, 4) : 0m;
                decimal maximo = rows.Where(r => r.Cantidad > 0).Select(r => r.Costo / r.Cantidad).DefaultIfEmpty(0m).Max();
                maximo = Math.Round(maximo, 4);
                var ultRow = rows.OrderByDescending(r => r.Fecha).First();
                decimal ultima = ultRow.Cantidad > 0 ? Math.Round(ultRow.Costo / ultRow.Cantidad, 4) : 0m;

                // Desglose por proveedor.
                var desglose = rows
                    .GroupBy(r => provMap.TryGetValue((r.IdOc, idMat), out var p) ? p : "SIN PROVEEDOR")
                    .Select(g =>
                    {
                        decimal cant = g.Sum(x => x.Cantidad);
                        decimal total = g.Sum(x => x.Costo);
                        return new ProveedorCostoDto
                        {
                            Proveedor = g.Key,
                            Cantidad = cant,
                            TotalPagado = Math.Round(total, 2),
                            PrecioUnitario = cant > 0 ? Math.Round(total / cant, 4) : 0m
                        };
                    })
                    .OrderByDescending(p => p.Cantidad)
                    .ToList();

                bool parcial = !oldestByMat.TryGetValue(idMat, out var min) || (min.HasValue && min.Value > fechaInicio);

                resultado.Add(new CostoPonderadoDto
                {
                    IdMaterial = idMat,
                    PromedioPonderado = promedio,
                    UltimaCompra = ultima,
                    Maximo = maximo,
                    CantidadTotal = cantTotal,
                    CostoTotal = Math.Round(costoTotal, 2),
                    VentanaMeses = ventana,
                    PeriodoInicio = fechaInicio.ToString("yyyy-MM-dd"),
                    PeriodoFin = hoy.ToString("yyyy-MM-dd"),
                    Parcial = parcial,
                    SinDatos = false,
                    Desglose = desglose
                });
            }

            return resultado;
        }

        private static CostoPonderadoDto EmptyDto(int idMaterial, int ventana, DateOnly fechaInicio) => new()
        {
            IdMaterial = idMaterial,
            VentanaMeses = ventana,
            PeriodoInicio = fechaInicio.ToString("yyyy-MM-dd"),
            PeriodoFin = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd"),
            SinDatos = true,
            Parcial = true
        };
    }
}
