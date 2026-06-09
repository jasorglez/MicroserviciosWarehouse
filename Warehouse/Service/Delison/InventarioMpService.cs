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

    public interface IInventarioMpService
    {
        // Vista GERENCIAL (sidebar = todas las sucursales): columnas = sucursales.
        Task<InventarioMpVistaDto> GetGerencial(int idCompany);
        // Vista POR SUCURSAL: columnas = departamentos (solo los que tienen datos).
        Task<InventarioMpVistaDto> GetPorSucursal(int idCompany, int idSucursal);
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

        public async Task<InventarioMpVistaDto> GetGerencial(int idCompany)
        {
            var vista = new InventarioMpVistaDto();
            var conn = _context.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await ((System.Data.Common.DbConnection)conn).OpenAsync();

            // 1) Filas = todas las MP de la empresa.
            var filas = await GetMateriasPrimas(conn, idCompany);
            var filaPorMat = filas.ToDictionary(f => f.IdMaterial);

            // 2) Sumas por (material, sucursal). Columnas = sucursales presentes.
            var columnas = new Dictionary<int, string>();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT i.id_material, i.id_sucursal, b.name AS sucursal, SUM(i.cantidad) AS cant
                    FROM warehouses.Delison.inventario_mp i
                    INNER JOIN smp.dbo.Branchs b ON b.id = i.id_sucursal
                    WHERE i.active = 1 AND i.id_company = @c
                    GROUP BY i.id_material, i.id_sucursal, b.name";
                AddParam(cmd, "@c", idCompany);
                using var r = await ((System.Data.Common.DbCommand)cmd).ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    var idMat = r.GetInt32(0);
                    var idSuc = r.GetInt32(1);
                    var sucNombre = r.IsDBNull(2) ? $"Sucursal {idSuc}" : r.GetString(2);
                    var cant = r.IsDBNull(3) ? 0m : r.GetDecimal(3);

                    if (!columnas.ContainsKey(idSuc)) columnas[idSuc] = sucNombre;
                    if (filaPorMat.TryGetValue(idMat, out var fila))
                    {
                        fila.Valores[idSuc] = (fila.Valores.TryGetValue(idSuc, out var v) ? v : 0m) + cant;
                        fila.Total += cant;
                    }
                }
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

            // Sumas por (material, departamento) de ESA sucursal. Columnas = departamentos
            // con datos (los que están en 0 en todas las MP no aparecen). Nombre desde Roles.
            var columnas = new Dictionary<int, string>();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT i.id_departamento,
                           LTRIM(RTRIM(COALESCE(rol.Description, ''))) AS departamento,
                           i.id_material, SUM(i.cantidad) AS cant
                    FROM warehouses.Delison.inventario_mp i
                    LEFT JOIN security.dbo.Roles rol ON rol.id = i.id_departamento
                    WHERE i.active = 1 AND i.id_company = @c AND i.id_sucursal = @s
                    GROUP BY i.id_departamento, rol.Description, i.id_material";
                AddParam(cmd, "@c", idCompany);
                AddParam(cmd, "@s", idSucursal);
                using var r = await ((System.Data.Common.DbCommand)cmd).ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    var idDep = r.GetInt32(0);
                    var depNombre = r.IsDBNull(1) || string.IsNullOrWhiteSpace(r.GetString(1))
                        ? $"Departamento {idDep}" : r.GetString(1);
                    var idMat = r.GetInt32(2);
                    var cant  = r.IsDBNull(3) ? 0m : r.GetDecimal(3);

                    if (!columnas.ContainsKey(idDep)) columnas[idDep] = depNombre;
                    if (filaPorMat.TryGetValue(idMat, out var fila))
                    {
                        fila.Valores[idDep] = (fila.Valores.TryGetValue(idDep, out var v) ? v : 0m) + cant;
                        fila.Total += cant;
                    }
                }
            }

            vista.Columnas = columnas
                .OrderBy(kv => kv.Value)
                .Select(kv => new InventarioMpColumnaDto { Id = kv.Key, Nombre = kv.Value })
                .ToList();
            vista.Filas = filas;
            return vista;
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
