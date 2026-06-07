using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;
using Warehouse.Models.DTOs;

namespace Warehouse.Service.Delison
{
    public interface IEmpaqueMedidaService
    {
        Task<List<EmpaqueMedidaDelison>> GetByEmpaque(int idEmpaque);
        Task<List<EmpaqueMedidaDelison>> SaveByEmpaque(EmpaqueMedidaSaveDto dto);
        Task DeleteByEmpaqueIds(IEnumerable<int> idsEmpaque);
    }

    public class EmpaqueMedidaService : IEmpaqueMedidaService
    {
        private readonly DbWarehouseContext _context;

        public EmpaqueMedidaService(DbWarehouseContext context)
        {
            _context = context;
        }

        public async Task<List<EmpaqueMedidaDelison>> GetByEmpaque(int idEmpaque)
        {
            return await _context.EmpaqueMedidas
                .Where(m => m.IdEmpaque == idEmpaque && m.Active)
                .OrderBy(m => m.Id)
                .ToListAsync();
        }

        /// <summary>Borra medidas de varias presentaciones (al eliminar/reemplazar empaques).</summary>
        public async Task DeleteByEmpaqueIds(IEnumerable<int> idsEmpaque)
        {
            var ids = idsEmpaque?.ToList() ?? new List<int>();
            if (ids.Count == 0) return;
            var rows = await _context.EmpaqueMedidas.Where(m => ids.Contains(m.IdEmpaque)).ToListAsync();
            if (rows.Count > 0) { _context.EmpaqueMedidas.RemoveRange(rows); await _context.SaveChangesAsync(); }
        }

        /// <summary>
        /// Reemplaza TODAS las medidas del proveedor: borra las existentes e inserta las nuevas.
        /// Solo inserta filas con al menos un dato (la fila vacía final del front no llega aquí,
        /// pero se filtra por seguridad).
        /// </summary>
        public async Task<List<EmpaqueMedidaDelison>> SaveByEmpaque(EmpaqueMedidaSaveDto dto)
        {
            if (dto == null || dto.IdEmpaque <= 0)
                return new List<EmpaqueMedidaDelison>();

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1) Borrado físico de las filas actuales de la presentación.
                var existing = await _context.EmpaqueMedidas
                    .Where(m => m.IdEmpaque == dto.IdEmpaque)
                    .ToListAsync();
                if (existing.Count > 0)
                    _context.EmpaqueMedidas.RemoveRange(existing);

                // 2) Inserta las filas con datos (descarta filas totalmente vacías).
                var items = (dto.Items ?? new List<EmpaqueMedidaItemDto>())
                    .Where(i => i.Medida.HasValue || i.IdUnidad.HasValue || i.IdDimension.HasValue)
                    .Select(i => new EmpaqueMedidaDelison
                    {
                        IdEmpaque    = dto.IdEmpaque,
                        Medida       = i.Medida,
                        IdUnidad     = i.IdUnidad,
                        IdDimension  = i.IdDimension,
                        Active       = true,
                        DateModified = DateTime.Now
                    })
                    .ToList();

                if (items.Count > 0)
                    await _context.EmpaqueMedidas.AddRangeAsync(items);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return items;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}
