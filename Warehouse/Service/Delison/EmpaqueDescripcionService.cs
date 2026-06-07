using System.Data;
using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;
using Warehouse.Models.DTOs;

namespace Warehouse.Service.Delison
{
    public interface IEmpaqueDescripcionService
    {
        Task<List<EmpaqueDescripcionDelison>> GetByProveedor(int idProveedorTabla);
        Task<List<EmpaqueDescripcionDelison>> SaveByProveedor(EmpaqueDescripcionSaveDto dto);
        Task<List<ProveedorPresentacionesDto>> GetPresentacionesByMaterial(int idMaterial);
    }

    public class EmpaqueDescripcionService : IEmpaqueDescripcionService
    {
        private readonly DbWarehouseContext _context;

        public EmpaqueDescripcionService(DbWarehouseContext context)
        {
            _context = context;
        }

        public async Task<List<EmpaqueDescripcionDelison>> GetByProveedor(int idProveedorTabla)
        {
            return await _context.EmpaqueDescripciones
                .Where(e => e.IdProveedorTabla == idProveedorTabla && e.Active)
                .OrderBy(e => e.Id)
                .ToListAsync();
        }

        /// <summary>
        /// Reemplaza TODAS las presentaciones del proveedor: borra las anteriores (y en cascada sus
        /// medidas/peso) e inserta las nuevas. Devuelve las insertadas con sus ids (en orden de Items).
        /// </summary>
        public async Task<List<EmpaqueDescripcionDelison>> SaveByProveedor(EmpaqueDescripcionSaveDto dto)
        {
            if (dto == null || dto.IdProveedorTabla <= 0)
                return new List<EmpaqueDescripcionDelison>();

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1) Presentaciones actuales del proveedor.
                var existing = await _context.EmpaqueDescripciones
                    .Where(e => e.IdProveedorTabla == dto.IdProveedorTabla)
                    .ToListAsync();
                var oldIds = existing.Select(e => e.Id).ToList();

                // 2) Cascada: borrar medidas/peso de esas presentaciones.
                if (oldIds.Count > 0)
                {
                    var meds = await _context.EmpaqueMedidas.Where(m => oldIds.Contains(m.IdEmpaque)).ToListAsync();
                    if (meds.Count > 0) _context.EmpaqueMedidas.RemoveRange(meds);
                    var pvs = await _context.EmpaquePesosVolumenes.Where(p => oldIds.Contains(p.IdEmpaque)).ToListAsync();
                    if (pvs.Count > 0) _context.EmpaquePesosVolumenes.RemoveRange(pvs);
                    _context.EmpaqueDescripciones.RemoveRange(existing);
                    await _context.SaveChangesAsync();
                }

                // 3) Insertar las presentaciones nuevas (en el orden recibido).
                var result = new List<EmpaqueDescripcionDelison>();
                foreach (var i in (dto.Items ?? new List<EmpaqueDescripcionItemDto>()))
                {
                    var row = new EmpaqueDescripcionDelison
                    {
                        IdProveedorTabla     = dto.IdProveedorTabla,
                        IdDescripcionEmpaque = i.IdDescripcionEmpaque,
                        PiezaXPaquete        = i.PiezaXPaquete,
                        Active               = true,
                        DateModified         = DateTime.Now
                    };
                    _context.EmpaqueDescripciones.Add(row);
                    result.Add(row);
                }
                await _context.SaveChangesAsync();   // asigna ids (en orden de inserción)

                await tx.CommitAsync();
                return result;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Para el panel de Requisiciones: todos los proveedores del material con su compra mínima
        /// y sus presentaciones (cada una con su peso/volumen convertido a base kg/L).
        /// </summary>
        public async Task<List<ProveedorPresentacionesDto>> GetPresentacionesByMaterial(int idMaterial)
        {
            const string sql = @"
                SELECT
                    pxt.[Id]              AS idProveedorTabla,
                    pxt.[id_tabla]        AS idProvider,
                    ISNULL(pxt.[minima_compra], 0) AS minCompra,
                    ed.[id]              AS idEmpaque,
                    ed.[id_descripcion_empaque] AS idDescripcionEmpaque,
                    de.[descripcion]     AS descripcionEmpaque,
                    ed.[pieza_x_paquete] AS piezaXPaquete,
                    epv.[medida]         AS medida,
                    pv.[abreviatura]     AS unidadAbrev,
                    pv.[tipo]            AS tipo,
                    pv.[factor_base]     AS factorBase
                FROM warehouses.dbo.proveedorxtablas pxt
                INNER JOIN warehouses.Delison.empaque_descripcion ed
                        ON ed.[id_proveedor_tabla] = pxt.[Id] AND ed.[active] = 1
                LEFT  JOIN warehouses.Delison.descripcion_empaque de
                        ON de.[id] = ed.[id_descripcion_empaque]
                LEFT  JOIN warehouses.Delison.empaque_peso_volumen epv
                        ON epv.[id_empaque] = ed.[id] AND epv.[active] = 1
                LEFT  JOIN warehouses.Delison.peso_volumen pv
                        ON pv.[id] = epv.[id_unidad]
                WHERE pxt.[campo1] = @idMaterial AND pxt.[type] = 'MATERIAL' AND pxt.[active] = 1
                ORDER BY pxt.[Id], ed.[id]";

            var byProv = new Dictionary<int, ProveedorPresentacionesDto>();

            var conn = _context.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            var p = cmd.CreateParameter(); p.ParameterName = "@idMaterial"; p.Value = idMaterial; cmd.Parameters.Add(p);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                int idProvTabla = Convert.ToInt32(reader["idProveedorTabla"]);
                if (!byProv.TryGetValue(idProvTabla, out var prov))
                {
                    prov = new ProveedorPresentacionesDto
                    {
                        IdProveedorTabla = idProvTabla,
                        IdProvider       = reader["idProvider"] == DBNull.Value ? 0 : Convert.ToInt32(reader["idProvider"]),
                        MinCompra        = reader["minCompra"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["minCompra"]),
                    };
                    byProv[idProvTabla] = prov;
                }

                decimal? medida     = reader["medida"]     == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["medida"]);
                decimal? factorBase = reader["factorBase"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["factorBase"]);
                prov.Presentaciones.Add(new PresentacionItemDto
                {
                    IdEmpaque            = Convert.ToInt32(reader["idEmpaque"]),
                    IdDescripcionEmpaque = reader["idDescripcionEmpaque"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["idDescripcionEmpaque"]),
                    DescripcionEmpaque   = reader["descripcionEmpaque"] == DBNull.Value ? null : reader["descripcionEmpaque"].ToString(),
                    PiezaXPaquete        = reader["piezaXPaquete"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["piezaXPaquete"]),
                    Medida               = medida,
                    UnidadAbrev          = reader["unidadAbrev"] == DBNull.Value ? null : reader["unidadAbrev"].ToString(),
                    Tipo                 = reader["tipo"] == DBNull.Value ? null : reader["tipo"].ToString(),
                    FactorBase           = factorBase,
                    MedidaBase           = (medida.HasValue && factorBase.HasValue) ? medida.Value * factorBase.Value : (decimal?)null,
                });
            }

            return byProv.Values.ToList();
        }
    }
}
