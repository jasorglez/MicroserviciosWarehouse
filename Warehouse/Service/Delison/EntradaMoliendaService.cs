using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;

namespace Warehouse.Service.Delison
{
    // Resumen de una entrada (por lote) para el Nivel 2 "Entradas" simple del Almacén Molienda (1ra/2da Fase).
    public class EntradaResumenDto
    {
        public string FolioEntrada { get; set; } = "";
        public string Lote { get; set; } = "";
        public string? Fecha { get; set; }   // yyyy-MM-dd
        public decimal Cantidad { get; set; }
        public string? Usuario { get; set; }
    }

    public interface IEntradaMoliendaService
    {
        Task<List<EntradaMoliendaDelison>> GetByOc(int idOc);
        Task<List<EntradaMoliendaDelison>> GetByOcAndMaterial(int idOc, int idMaterial);
        Task<List<EntradaMoliendaDelison>> GetByEntregaAndMaterial(int idEntrega, int idMaterial);
        Task<EntradaMoliendaDelison?> GetById(int id);
        Task<EntradaMoliendaDelison> Create(EntradaMoliendaDelison data);
        Task<EntradaMoliendaDelison?> Update(int id, EntradaMoliendaDelison data);
        Task<bool> Delete(int id);
        Task<List<EntradaResumenDto>> GetResumenByMaterialAndSucursal(int idMaterial, int idSucursal);
    }

    public class EntradaMoliendaService : IEntradaMoliendaService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<EntradaMoliendaService> _logger;

        public EntradaMoliendaService(DbWarehouseContext context, ILogger<EntradaMoliendaService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        public async Task<List<EntradaMoliendaDelison>> GetByOc(int idOc)
        {
            return await _context.EntradasMolienda
                .Where(e => e.IdOc == idOc && e.Active)
                .OrderBy(e => e.FechaRecepcion)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<EntradaMoliendaDelison>> GetByOcAndMaterial(int idOc, int idMaterial)
        {
            return await _context.EntradasMolienda
                .Where(e => e.IdOc == idOc && e.Active
                         && (e.IdMaterial == null || e.IdMaterial == idMaterial))
                .OrderBy(e => e.FechaRecepcion)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<EntradaMoliendaDelison>> GetByEntregaAndMaterial(int idEntrega, int idMaterial)
        {
            return await _context.EntradasMolienda
                .Where(e => e.IdEntrega == idEntrega && e.Active
                         && (e.IdMaterial == null || e.IdMaterial == idMaterial))
                .OrderBy(e => e.FechaRecepcion)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<EntradaMoliendaDelison?> GetById(int id)
        {
            return await _context.EntradasMolienda
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<EntradaMoliendaDelison> Create(EntradaMoliendaDelison data)
        {
            data.Active       = true;
            data.DateModified = DateTime.UtcNow;
            _context.EntradasMolienda.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<EntradaMoliendaDelison?> Update(int id, EntradaMoliendaDelison data)
        {
            var existing = await _context.EntradasMolienda.FindAsync(id);
            if (existing == null) return null;

            existing.IdEntrega       = data.IdEntrega;
            existing.FechaRecepcion  = data.FechaRecepcion;
            existing.CantidadEntrada = data.CantidadEntrada;
            existing.Bultos          = data.Bultos;
            existing.RevisionConfigu = data.RevisionConfigu;
            existing.Pago            = data.Pago;
            existing.FechaPago       = data.FechaPago;
            existing.NotaFactura     = data.NotaFactura;
            existing.Usuario         = data.Usuario;
            existing.Liberacion      = data.Liberacion;
            existing.Close           = data.Close;
            existing.Comentario      = data.Comentario;
            existing.FolioEntrega    = data.FolioEntrega;
            existing.Active          = data.Active;
            existing.DateModified    = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> Delete(int id)
        {
            var existing = await _context.EntradasMolienda.FindAsync(id);
            if (existing == null) return false;
            existing.Active       = false;
            existing.DateModified = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<EntradaResumenDto>> GetResumenByMaterialAndSucursal(int idMaterial, int idSucursal)
        {
            if (idMaterial <= 0 || idSucursal <= 0) return new List<EntradaResumenDto>();

            // 1. Entradas liberadas del material.
            var entradas = await _context.EntradasMolienda
                .Where(e => e.Active && e.Liberacion && e.IdMaterial == idMaterial)
                .Select(e => new { e.Id, e.IdOc, e.FolioEntrega, e.FechaRecepcion, e.Usuario })
                .ToListAsync();
            if (entradas.Count == 0) return new List<EntradaResumenDto>();

            // 2. Resolver sucursal de cada OC (CR: branch en la REQUIS padre).
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
            if (entradasFiltradas.Count == 0) return new List<EntradaResumenDto>();

            var entradaIds  = entradasFiltradas.Select(e => e.Id).ToList();
            var entradaById = entradasFiltradas.ToDictionary(e => e.Id);

            // 4. Lotes de esas entradas (datos_externos_molienda).
            var lotes = await _context.DatosExternosMolienda
                .Where(d => d.Active && entradaIds.Contains(d.IdEntrada))
                .Select(d => new { d.IdEntrada, d.Lote, d.CantidadXLote })
                .OrderBy(d => d.IdEntrada)
                .ToListAsync();

            // Si no hay lotes, devolver una fila por entrada con lote vacío.
            if (lotes.Count == 0)
            {
                return entradasFiltradas
                    .OrderBy(e => e.FechaRecepcion)
                    .Select(e => new EntradaResumenDto
                    {
                        FolioEntrada = e.FolioEntrega ?? "",
                        Lote         = "",
                        Fecha        = e.FechaRecepcion?.ToString("yyyy-MM-dd"),
                        Cantidad     = 0,
                        Usuario      = e.Usuario
                    }).ToList();
            }

            return lotes.Select(l =>
            {
                entradaById.TryGetValue(l.IdEntrada, out var ent);
                return new EntradaResumenDto
                {
                    FolioEntrada = ent?.FolioEntrega ?? "",
                    Lote         = l.Lote ?? "",
                    Fecha        = ent?.FechaRecepcion?.ToString("yyyy-MM-dd"),
                    Cantidad     = l.CantidadXLote ?? 0,
                    Usuario      = ent?.Usuario
                };
            }).ToList();
        }
    }
}
