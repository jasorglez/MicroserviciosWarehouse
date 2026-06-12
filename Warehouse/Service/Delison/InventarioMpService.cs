using System.Data;
using Microsoft.EntityFrameworkCore;

namespace Warehouse.Service.Delison
{
    // ── DTOs de respuesta (pivote) ──────────────────────────────────────────
    // El backend arma la matriz: una fila por materia prima (TODAS, aunque estén
    // en 0) y una columna dinámica por dimensión (sucursal en gerencial,
    // departamento en la vista por sucursal). El frontend solo renderiza.
    public class InventarioMpColumnaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
    }

    public class InventarioMpFilaDto
    {
        public int IdMaterial { get; set; }
        public string Articulo { get; set; } = "";
        public decimal Total { get; set; }
        // clave = Id de la columna (sucursal o departamento), valor = cantidad
        public Dictionary<int, decimal> Valores { get; set; } = new();
    }

    public class InventarioMpVistaDto
    {
        public List<InventarioMpColumnaDto> Columnas { get; set; } = new();
        public List<InventarioMpFilaDto> Filas { get; set; } = new();
    }

    // ── Detalle de lotes al click en una celda (material × departamento × sucursal) ──
    public class InventarioMpDetalleDto
    {
        public decimal Total { get; set; }
        public List<InventarioMpLoteDto> Lotes { get; set; } = new();
    }

    // Una fila Nivel 1 = un lote DE UNA ENTRADA (datos_externos_molienda).
    public class InventarioMpLoteDto
    {
        public int IdEntrada { get; set; }
        public int IdDatoExterno { get; set; }
        public string Lote { get; set; } = "";
        public string FolioEntrada { get; set; } = "";
        public decimal CantidadInventario { get; set; }   // cantidad_x_lote − salidas (hoy 0)
        public List<InventarioMpMovimientoDto> Movimientos { get; set; } = new();
    }

    // Nivel 2 = movimiento (entrada o salida) del lote-entrada.
    public class InventarioMpMovimientoDto
    {
        public string Tipo { get; set; } = "ENTRADA";   // ENTRADA | SALIDA
        public string? Fecha { get; set; }              // yyyy-MM-dd
        public decimal? CantidadEntrada { get; set; }
        public decimal? CantidadSalida { get; set; }
        public string Quien { get; set; } = "";         // nombre de quien recibió/utilizó
    }

    public interface IInventarioMpService
    {
        // Vista GERENCIAL (sidebar = todas las sucursales): columnas = sucursales.
        Task<InventarioMpVistaDto> GetGerencial(int idCompany);
        // Vista POR SUCURSAL: columnas = departamentos (solo los que tienen datos).
        Task<InventarioMpVistaDto> GetPorSucursal(int idCompany, int idSucursal);
        // Detalle de lotes de una celda (material × departamento × sucursal).
        Task<InventarioMpDetalleDto> GetDetalle(int idMaterial, int idDepartamento, int idSucursal);
    }

    public class InventarioMpService : IInventarioMpService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<InventarioMpService> _logger;

        public InventarioMpService(DbWarehouseContext context, ILogger<InventarioMpService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        // ── Catálogo de materias primas de la empresa (TODAS, aunque estén en 0) ──
        // Categoría MATERIA PRIMA por empresa (no hardcodeamos el id 641).
        private async Task<List<InventarioMpFilaDto>> GetMateriasPrimas(IDbConnection conn, int idCompany)
        {
            var filas = new List<InventarioMpFilaDto>();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT m.id, LTRIM(RTRIM(COALESCE(NULLIF(m.description,''), m.insumo, ''))) AS nombre
                FROM warehouses.dbo.materials m
                WHERE m.id_company = @c AND m.active = 1
                  AND m.id_category IN (
                      SELECT id FROM warehouses.dbo.catalog
                      WHERE type = 'CATEGORY' AND description = 'MATERIA PRIMA' AND id_company = @c
                  )
                ORDER BY nombre";
            AddParam(cmd, "@c", idCompany);
            using var r = await ((System.Data.Common.DbCommand)cmd).ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                filas.Add(new InventarioMpFilaDto
                {
                    IdMaterial = r.GetInt32(0),
                    Articulo   = r.IsDBNull(1) ? "" : r.GetString(1),
                    Total      = 0m,
                    Valores    = new Dictionary<int, decimal>()
                });
            }
            return filas;
        }

        // Un lote (datos_externos) de una entrada liberada, ya resuelto a sucursal+departamento+material.
        private sealed class MovLoteVivo
        {
            public int IdMaterial;
            public int IdSucursal;
            public int IdDepartamento;
            public decimal Cantidad;
        }

        // Inventario EN VIVO (NO snapshot): lotes (datos_externos_molienda) de entradas LIBERADAS,
        // resueltos a sucursal+departamento como en AcumularInventarioMp. Si se borran las entradas/lotes
        // de origen, esto da 0 automáticamente. − salidas (actividades que gastan MP; aún no existen).
        private async Task<List<MovLoteVivo>> GetMovimientosVivos()
        {
            // 1) Lotes + entrada (liberadas, activas, con material).
            var lotes = await (from d in _context.DatosExternosMolienda
                               where d.Active
                               join e in _context.EntradasMolienda on d.IdEntrada equals e.Id
                               where e.Active && e.Liberacion && e.IdMaterial != null
                               select new { DatoId = d.Id, e.IdOc, IdMaterial = e.IdMaterial!.Value, Cant = d.CantidadXLote ?? 0m })
                              .ToListAsync();
            if (lotes.Count == 0) return new List<MovLoteVivo>();

            // Salidas (consumo) por lote → para descontar del inventario.
            var datoIds = lotes.Select(l => l.DatoId).Distinct().ToList();
            var salidasPorLote = await _context.SalidasMp
                .Where(s => s.Active && datoIds.Contains(s.IdDatoExterno))
                .GroupBy(s => s.IdDatoExterno)
                .Select(g => new { IdDato = g.Key, Sum = g.Sum(x => x.Cantidad) })
                .ToDictionaryAsync(x => x.IdDato, x => x.Sum);

            // 2) OC → (departamento, branch). CR: branch en la REQUIS padre (id_req).
            var ocIds = lotes.Select(l => l.IdOc).Distinct().ToList();
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
                    && reqBranch.TryGetValue(o.IdReq.Value, out var rb) && rb > 0)
                    branch = rb;
                ocMap[o.Id] = (o.IdDepartament, branch);
            }

            var movs = new List<MovLoteVivo>();
            foreach (var l in lotes)
            {
                if (!ocMap.TryGetValue(l.IdOc, out var m)) continue;
                var gastado = salidasPorLote.TryGetValue(l.DatoId, out var sg) ? sg : 0m;
                var neto = l.Cant - gastado;   // entrada del lote − salidas
                if (neto <= 0) continue;        // lote agotado → no suma al inventario
                movs.Add(new MovLoteVivo { IdMaterial = l.IdMaterial, IdSucursal = m.branch, IdDepartamento = m.depto, Cantidad = neto });
            }
            return movs;
        }

        private async Task<Dictionary<int, string>> GetNombresSucursales(IDbConnection conn, List<int> ids)
        {
            var map = new Dictionary<int, string>();
            if (ids.Count == 0) return map;
            using var cmd = conn.CreateCommand();
            var ins = string.Join(",", ids.Select((_, i) => "@b" + i));
            cmd.CommandText = $"SELECT id, name FROM smp.dbo.Branchs WHERE id IN ({ins})";
            for (int i = 0; i < ids.Count; i++) AddParam(cmd, "@b" + i, ids[i]);
            using var r = await ((System.Data.Common.DbCommand)cmd).ExecuteReaderAsync();
            while (await r.ReadAsync()) map[r.GetInt32(0)] = r.IsDBNull(1) ? "" : r.GetString(1);
            return map;
        }

        private async Task<Dictionary<int, string>> GetNombresDepartamentos(IDbConnection conn, List<int> ids)
        {
            var map = new Dictionary<int, string>();
            if (ids.Count == 0) return map;
            using var cmd = conn.CreateCommand();
            var ins = string.Join(",", ids.Select((_, i) => "@d" + i));
            cmd.CommandText = $"SELECT id, LTRIM(RTRIM(COALESCE(Description,''))) FROM security.dbo.Roles WHERE id IN ({ins})";
            for (int i = 0; i < ids.Count; i++) AddParam(cmd, "@d" + i, ids[i]);
            using var r = await ((System.Data.Common.DbCommand)cmd).ExecuteReaderAsync();
            while (await r.ReadAsync()) map[r.GetInt32(0)] = r.IsDBNull(1) ? "" : r.GetString(1);
            return map;
        }

        public async Task<InventarioMpVistaDto> GetGerencial(int idCompany)
        {
            var vista = new InventarioMpVistaDto();
            var conn = _context.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await ((System.Data.Common.DbConnection)conn).OpenAsync();

            // Filas = todas las MP de la empresa (filtro por empresa = solo sus materiales).
            var filas = await GetMateriasPrimas(conn, idCompany);
            var filaPorMat = filas.ToDictionary(f => f.IdMaterial);

            // Cálculo EN VIVO. Se agrega por (material, sucursal); solo cuenta a materiales de esta empresa.
            var movs = await GetMovimientosVivos();
            var sucNombres = await GetNombresSucursales(conn, movs.Select(m => m.IdSucursal).Distinct().Where(x => x > 0).ToList());

            var columnas = new Dictionary<int, string>();
            foreach (var m in movs)
            {
                if (!filaPorMat.TryGetValue(m.IdMaterial, out var fila)) continue;
                if (!columnas.ContainsKey(m.IdSucursal))
                    columnas[m.IdSucursal] = sucNombres.TryGetValue(m.IdSucursal, out var n) && !string.IsNullOrWhiteSpace(n)
                        ? n : $"Sucursal {m.IdSucursal}";
                fila.Valores[m.IdSucursal] = (fila.Valores.TryGetValue(m.IdSucursal, out var v) ? v : 0m) + m.Cantidad;
                fila.Total += m.Cantidad;
            }

            vista.Columnas = columnas
                .OrderBy(kv => kv.Value)
                .Select(kv => new InventarioMpColumnaDto { Id = kv.Key, Nombre = kv.Value })
                .ToList();
            vista.Filas = filas;
            return vista;
        }

        public async Task<InventarioMpVistaDto> GetPorSucursal(int idCompany, int idSucursal)
        {
            var vista = new InventarioMpVistaDto();
            var conn = _context.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await ((System.Data.Common.DbConnection)conn).OpenAsync();

            var filas = await GetMateriasPrimas(conn, idCompany);
            var filaPorMat = filas.ToDictionary(f => f.IdMaterial);

            // Cálculo EN VIVO de ESA sucursal. Columnas = departamentos con datos. Nombre desde Roles.
            var movs = (await GetMovimientosVivos()).Where(m => m.IdSucursal == idSucursal).ToList();
            var depNombres = await GetNombresDepartamentos(conn, movs.Select(m => m.IdDepartamento).Distinct().Where(x => x > 0).ToList());

            var columnas = new Dictionary<int, string>();
            foreach (var m in movs)
            {
                if (!filaPorMat.TryGetValue(m.IdMaterial, out var fila)) continue;
                if (!columnas.ContainsKey(m.IdDepartamento))
                    columnas[m.IdDepartamento] = depNombres.TryGetValue(m.IdDepartamento, out var n) && !string.IsNullOrWhiteSpace(n)
                        ? n : $"Departamento {m.IdDepartamento}";
                fila.Valores[m.IdDepartamento] = (fila.Valores.TryGetValue(m.IdDepartamento, out var v) ? v : 0m) + m.Cantidad;
                fila.Total += m.Cantidad;
            }

            vista.Columnas = columnas
                .OrderBy(kv => kv.Value)
                .Select(kv => new InventarioMpColumnaDto { Id = kv.Key, Nombre = kv.Value })
                .ToList();
            vista.Filas = filas;
            return vista;
        }

        // ── Detalle de lotes de una celda (material × departamento × sucursal) ──
        // Espejo de AcumularInventarioMp: entradas LIBERADAS de ese material, cuya OC tiene ese
        // departamento y cuyo branch (OC=id_reference; CR=REQUIS padre) es la sucursal. Por cada
        // entrada que cumple → sus lotes (datos_externos_molienda) son las filas de Nivel 1.
        // Salidas aún no existen → la cantidad inventario = cantidad_x_lote, sin restar nada.
        public async Task<InventarioMpDetalleDto> GetDetalle(int idMaterial, int idDepartamento, int idSucursal)
        {
            var dto = new InventarioMpDetalleDto();
            if (idMaterial <= 0 || idDepartamento <= 0 || idSucursal <= 0) return dto;

            // 1) Entradas liberadas del material.
            var entradas = await _context.EntradasMolienda
                .Where(e => e.Active && e.Liberacion && e.IdMaterial == idMaterial)
                .Select(e => new { e.Id, e.IdOc, e.FolioEntrega, e.FechaRecepcion, e.Usuario })
                .ToListAsync();
            if (entradas.Count == 0) return dto;

            // 2) Resolver OC → (departamento, branch). Branch: OC=id_reference, CR=id_reference de la REQUIS padre.
            var ocIds = entradas.Select(e => e.IdOc).Distinct().ToList();
            var ocs = await _context.Ocandreqs
                .Where(o => ocIds.Contains(o.Id))
                .Select(o => new { o.Id, o.IdDepartament, o.Type, o.IdReq, o.IdReference })
                .ToListAsync();

            var reqIds = ocs.Where(o => string.Equals(o.Type, "CR", StringComparison.OrdinalIgnoreCase) && o.IdReq.HasValue)
                            .Select(o => o.IdReq!.Value).Distinct().ToList();
            var reqBranch = reqIds.Count > 0
                ? await _context.Ocandreqs.Where(r => reqIds.Contains(r.Id))
                    .Select(r => new { r.Id, r.IdReference }).ToDictionaryAsync(x => x.Id, x => x.IdReference)
                : new Dictionary<int, int>();

            // idOc → (depto, branch)
            var ocMap = new Dictionary<int, (int depto, int branch)>();
            foreach (var o in ocs)
            {
                int branch = o.IdReference;
                if (string.Equals(o.Type, "CR", StringComparison.OrdinalIgnoreCase) && o.IdReq.HasValue
                    && reqBranch.TryGetValue(o.IdReq.Value, out var rb) && rb > 0)
                    branch = rb;
                ocMap[o.Id] = (o.IdDepartament, branch);
            }

            // 3) Entradas que caen en este departamento + sucursal.
            var entradasFiltradas = entradas
                .Where(e => ocMap.TryGetValue(e.IdOc, out var m) && m.depto == idDepartamento && m.branch == idSucursal)
                .ToList();
            if (entradasFiltradas.Count == 0) return dto;

            var entradaIds = entradasFiltradas.Select(e => e.Id).ToList();
            var entradaById = entradasFiltradas.ToDictionary(e => e.Id);

            // 4) Lotes (datos_externos) de esas entradas.
            var lotes = await _context.DatosExternosMolienda
                .Where(d => d.Active && entradaIds.Contains(d.IdEntrada))
                .Select(d => new { d.Id, d.IdEntrada, d.Lote, d.CantidadXLote })
                .ToListAsync();

            // 5) Salidas (consumo) de esos lotes → movimientos de salida + descuento.
            var datoIds = lotes.Select(l => l.Id).ToList();
            var salidas = await _context.SalidasMp
                .Where(s => s.Active && datoIds.Contains(s.IdDatoExterno))
                .Select(s => new { s.Id, s.IdDatoExterno, s.Cantidad, s.Fecha, s.Usuario })
                .ToListAsync();
            var salidasPorLote = salidas.GroupBy(s => s.IdDatoExterno)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Fecha).ThenBy(x => x.Id).ToList());

            foreach (var l in lotes.OrderBy(l => l.IdEntrada).ThenBy(l => l.Id))
            {
                var entrada = l.CantidadXLote ?? 0m;
                var lotSalidas = salidasPorLote.TryGetValue(l.Id, out var ls) ? ls : new();
                var gastado = lotSalidas.Sum(x => x.Cantidad);
                var neto = entrada - gastado;        // entrada del lote − salidas
                if (neto <= 0) continue;             // lote en 0 desaparece
                entradaById.TryGetValue(l.IdEntrada, out var ent);

                var movimientos = new List<InventarioMpMovimientoDto>
                {
                    // ENTRADA (el ingreso original del lote).
                    new InventarioMpMovimientoDto
                    {
                        Tipo = "ENTRADA",
                        Fecha = ent?.FechaRecepcion?.ToString("yyyy-MM-dd"),
                        CantidadEntrada = entrada,
                        CantidadSalida = null,
                        Quien = ent?.Usuario ?? ""
                    }
                };
                // SALIDAS (cada consumo registrado).
                foreach (var s in lotSalidas)
                {
                    movimientos.Add(new InventarioMpMovimientoDto
                    {
                        Tipo = "SALIDA",
                        Fecha = s.Fecha?.ToString("yyyy-MM-dd"),
                        CantidadEntrada = null,
                        CantidadSalida = s.Cantidad,
                        Quien = s.Usuario ?? ""
                    });
                }

                dto.Lotes.Add(new InventarioMpLoteDto
                {
                    IdEntrada = l.IdEntrada,
                    IdDatoExterno = l.Id,
                    Lote = l.Lote ?? "",
                    FolioEntrada = ent?.FolioEntrega ?? "",
                    CantidadInventario = neto,   // entrada − salidas
                    Movimientos = movimientos
                });
                dto.Total += neto;
            }

            return dto;
        }

        private static void AddParam(IDbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? System.DBNull.Value;
            cmd.Parameters.Add(p);
        }
    }
}
