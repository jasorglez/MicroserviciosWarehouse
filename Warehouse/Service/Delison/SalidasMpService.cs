using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;

namespace Warehouse.Service.Delison
{
    // Un lote disponible para consumir (modal FEFO de Molienda Nivel 3).
    // Resumen de una salida para el Nivel 2 "Salidas" del Almacén Molienda.
    public class SalidaResumenDto
    {
        public string FolioEntrada { get; set; } = "";
        public string Lote { get; set; } = "";
        public string? Fecha { get; set; }     // yyyy-MM-dd
        public decimal Cantidad { get; set; }
        public string? Usuario { get; set; }
    }

    public class LoteDisponibleDto
    {
        public int IdDatoExterno { get; set; }
        public int IdEntrada { get; set; }
        public string Lote { get; set; } = "";
        public string FolioEntrada { get; set; } = "";
        public decimal CantidadDisponible { get; set; }   // cantidad_x_lote − salidas previas
        public string? FechaEntrada { get; set; }          // yyyy-MM-dd
        public int? CaducidadMeses { get; set; }           // la capturada al entrar
        public int? CaducidadRestanteMeses { get; set; }   // dinámica = meses hasta vencer
        public string? FechaCaducidad { get; set; }        // yyyy-MM-dd (fecha_entrada + caducidad_meses)
        public string Proveedor { get; set; } = "";
    }

    public interface ISalidasMpService
    {
        // Lotes disponibles para consumir un material en sucursal+departamento, FEFO (menor caducidad primero).
        Task<List<LoteDisponibleDto>> GetLotesDisponibles(int idMaterial, int idDepartamento, int idSucursal);
        // Resumen de salidas para el Nivel 2 "Salidas" del Almacén Molienda.
        Task<List<SalidaResumenDto>> GetResumenByMaterialAndSucursal(int idMaterial, int idSucursal);
        // CRUD de salidas (consumo por lote).
        Task<List<SalidasMpDelison>> GetByOrigen(string tipoOrigen, int idOrigen);
        Task<SalidasMpDelison> Create(SalidasMpDelison data);
        Task<SalidasMpDelison?> Update(int id, SalidasMpDelison data);
        Task<bool> Delete(int id);
    }

    public class SalidasMpService : ISalidasMpService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<SalidasMpService> _logger;

        public SalidasMpService(DbWarehouseContext context, ILogger<SalidasMpService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        public async Task<List<LoteDisponibleDto>> GetLotesDisponibles(int idMaterial, int idDepartamento, int idSucursal)
        {
            var result = new List<LoteDisponibleDto>();
            if (idMaterial <= 0 || idDepartamento <= 0 || idSucursal <= 0) return result;

            // 1) Entradas liberadas del material.
            var entradas = await _context.EntradasMolienda
                .Where(e => e.Active && e.Liberacion && e.IdMaterial == idMaterial)
                .Select(e => new { e.Id, e.IdOc, e.FolioEntrega, e.FechaRecepcion })
                .ToListAsync();
            if (entradas.Count == 0) return result;

            // 2) OC → (departamento, branch). CR: branch en la REQUIS padre.
            var ocIds = entradas.Select(e => e.IdOc).Distinct().ToList();
            var ocs = await _context.Ocandreqs.Where(o => ocIds.Contains(o.Id))
                .Select(o => new { o.Id, o.IdDepartament, o.Type, o.IdReq, o.IdReference }).ToListAsync();
            var reqIds = ocs.Where(o => string.Equals(o.Type, "CR", StringComparison.OrdinalIgnoreCase) && o.IdReq.HasValue)
                            .Select(o => o.IdReq!.Value).Distinct().ToList();
            var reqBranch = reqIds.Count > 0
                ? await _context.Ocandreqs.Where(r => reqIds.Contains(r.Id))
                    .Select(r => new { r.Id, r.IdReference }).ToDictionaryAsync(x => x.Id, x => x.IdReference)
                : new Dictionary<int, int>();
            var ocMap = new Dictionary<int, (int depto, int branch)>();
            foreach (var o in ocs)
            {
                int branch = o.IdReference;
                if (string.Equals(o.Type, "CR", StringComparison.OrdinalIgnoreCase) && o.IdReq.HasValue
                    && reqBranch.TryGetValue(o.IdReq.Value, out var rb) && rb > 0) branch = rb;
                ocMap[o.Id] = (o.IdDepartament, branch);
            }

            // 3) Entradas que caen en este departamento + sucursal.
            var entradasFiltradas = entradas
                .Where(e => ocMap.TryGetValue(e.IdOc, out var m) && m.depto == idDepartamento && m.branch == idSucursal)
                .ToList();
            if (entradasFiltradas.Count == 0) return result;
            var entradaIds = entradasFiltradas.Select(e => e.Id).ToList();
            var entradaById = entradasFiltradas.ToDictionary(e => e.Id);

            // Proveedor por OC (best-effort: name_provider del detalle).
            var provPorOc = await _context.Detailsreqoc
                .Where(d => d.Active == true && ocIds.Contains(d.IdMovement) && d.NameProvider != null)
                .GroupBy(d => d.IdMovement)
                .Select(g => new { IdOc = g.Key, Prov = g.Max(x => x.NameProvider) })
                .ToDictionaryAsync(x => x.IdOc, x => x.Prov);

            // 4) Lotes de esas entradas.
            var lotes = await _context.DatosExternosMolienda
                .Where(d => d.Active && entradaIds.Contains(d.IdEntrada))
                .Select(d => new { d.Id, d.IdEntrada, d.Lote, d.CantidadXLote, d.CaducidadMeses })
                .ToListAsync();
            if (lotes.Count == 0) return result;

            // 5) Salidas previas por lote (para el disponible).
            var datoIds = lotes.Select(l => l.Id).ToList();
            var salidasPorLote = await _context.SalidasMp
                .Where(s => s.Active && datoIds.Contains(s.IdDatoExterno))
                .GroupBy(s => s.IdDatoExterno)
                .Select(g => new { IdDato = g.Key, Sum = g.Sum(x => x.Cantidad) })
                .ToDictionaryAsync(x => x.IdDato, x => x.Sum);

            var today = DateOnly.FromDateTime(DateTime.Now);
            foreach (var l in lotes)
            {
                var gastado = salidasPorLote.TryGetValue(l.Id, out var s) ? s : 0m;
                var disponible = (l.CantidadXLote ?? 0m) - gastado;
                if (disponible <= 0) continue;   // lote agotado → no aparece

                entradaById.TryGetValue(l.IdEntrada, out var ent);
                var fechaEnt = ent?.FechaRecepcion;

                int? caducidadRestante = null;
                string? fechaCaducidad = null;
                if (fechaEnt.HasValue && l.CaducidadMeses.HasValue)
                {
                    var expiry = fechaEnt.Value.AddMonths(l.CaducidadMeses.Value);
                    fechaCaducidad = expiry.ToString("yyyy-MM-dd");
                    var meses = (expiry.Year - today.Year) * 12 + (expiry.Month - today.Month);
                    if (expiry.Day < today.Day) meses -= 1;
                    caducidadRestante = meses;
                }

                var proveedor = (ent != null && provPorOc.TryGetValue(ent.IdOc, out var pn)) ? (pn ?? "") : "";

                result.Add(new LoteDisponibleDto
                {
                    IdDatoExterno = l.Id,
                    IdEntrada = l.IdEntrada,
                    Lote = l.Lote ?? "",
                    FolioEntrada = ent?.FolioEntrega ?? "",
                    CantidadDisponible = disponible,
                    FechaEntrada = fechaEnt?.ToString("yyyy-MM-dd"),
                    CaducidadMeses = l.CaducidadMeses,
                    CaducidadRestanteMeses = caducidadRestante,
                    FechaCaducidad = fechaCaducidad,
                    Proveedor = proveedor
                });
            }

            // FEFO: menor fecha de caducidad primero; sin caducidad al final.
            return result
                .OrderBy(x => x.FechaCaducidad == null)
                .ThenBy(x => x.FechaCaducidad)
                .ThenBy(x => x.FolioEntrada)
                .ToList();
        }

        public async Task<List<SalidaResumenDto>> GetResumenByMaterialAndSucursal(int idMaterial, int idSucursal)
        {
            if (idMaterial <= 0 || idSucursal <= 0) return new List<SalidaResumenDto>();

            // 1. Entradas liberadas del material.
            var entradas = await _context.EntradasMolienda
                .Where(e => e.Active && e.Liberacion && e.IdMaterial == idMaterial)
                .Select(e => new { e.Id, e.IdOc, e.FolioEntrega })
                .ToListAsync();
            if (entradas.Count == 0) return new List<SalidaResumenDto>();

            // 2. Resolver sucursal de cada OC (igual que GetLotesDisponibles).
            var ocIds = entradas.Select(e => e.IdOc).Distinct().ToList();
            var ocs = await _context.Ocandreqs.Where(o => ocIds.Contains(o.Id))
                .Select(o => new { o.Id, o.Type, o.IdReq, o.IdReference }).ToListAsync();
            var reqIds = ocs.Where(o => string.Equals(o.Type, "CR", StringComparison.OrdinalIgnoreCase) && o.IdReq.HasValue)
                            .Select(o => o.IdReq!.Value).Distinct().ToList();
            var reqBranch = reqIds.Count > 0
                ? await _context.Ocandreqs.Where(r => reqIds.Contains(r.Id))
                    .Select(r => new { r.Id, r.IdReference }).ToDictionaryAsync(x => x.Id, x => x.IdReference)
                : new Dictionary<int, int>();
            var ocBranch = new Dictionary<int, int>();
            foreach (var o in ocs)
            {
                int branch = o.IdReference;
                if (string.Equals(o.Type, "CR", StringComparison.OrdinalIgnoreCase) && o.IdReq.HasValue
                    && reqBranch.TryGetValue(o.IdReq.Value, out var rb) && rb > 0) branch = rb;
                ocBranch[o.Id] = branch;
            }

            // 3. Filtrar entradas de esta sucursal.
            var entradasFiltradas = entradas
                .Where(e => ocBranch.TryGetValue(e.IdOc, out var b) && b == idSucursal)
                .ToList();
            if (entradasFiltradas.Count == 0) return new List<SalidaResumenDto>();

            var entradaIds = entradasFiltradas.Select(e => e.Id).ToList();
            var folioById  = entradasFiltradas.ToDictionary(e => e.Id, e => e.FolioEntrega ?? "");

            // 4. Lotes de esas entradas.
            var lotes = await _context.DatosExternosMolienda
                .Where(d => d.Active && entradaIds.Contains(d.IdEntrada))
                .Select(d => new { d.Id, d.IdEntrada, d.Lote })
                .ToListAsync();
            if (lotes.Count == 0) return new List<SalidaResumenDto>();

            var loteById = lotes.ToDictionary(l => l.Id);
            var datoIds  = lotes.Select(l => l.Id).ToList();

            // 5. Salidas de esos lotes.
            var salidas = await _context.SalidasMp
                .Where(s => s.Active && datoIds.Contains(s.IdDatoExterno))
                .OrderBy(s => s.Fecha).ThenBy(s => s.Id)
                .ToListAsync();

            return salidas.Select(s =>
            {
                loteById.TryGetValue(s.IdDatoExterno, out var lote);
                folioById.TryGetValue(lote?.IdEntrada ?? 0, out var folio);
                return new SalidaResumenDto
                {
                    FolioEntrada = folio ?? "",
                    Lote         = lote?.Lote ?? "",
                    Fecha        = s.Fecha?.ToString("yyyy-MM-dd"),
                    Cantidad     = s.Cantidad,
                    Usuario      = s.Usuario
                };
            }).ToList();
        }

        public async Task<List<SalidasMpDelison>> GetByOrigen(string tipoOrigen, int idOrigen)
        {
            return await _context.SalidasMp
                .Where(s => s.Active && s.TipoOrigen == tipoOrigen && s.IdOrigen == idOrigen)
                .OrderBy(s => s.Id)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<SalidasMpDelison> Create(SalidasMpDelison data)
        {
            data.Active = true;
            data.DateModified = DateTime.UtcNow;
            _context.SalidasMp.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<SalidasMpDelison?> Update(int id, SalidasMpDelison data)
        {
            var existing = await _context.SalidasMp.FindAsync(id);
            if (existing == null) return null;
            existing.IdDatoExterno = data.IdDatoExterno;
            existing.IdMaterial    = data.IdMaterial;
            existing.Cantidad      = data.Cantidad;
            existing.Fecha         = data.Fecha;
            existing.Usuario       = data.Usuario;
            existing.DateModified  = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> Delete(int id)
        {
            var existing = await _context.SalidasMp.FindAsync(id);
            if (existing == null) return false;
            existing.Active = false;
            existing.DateModified = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
