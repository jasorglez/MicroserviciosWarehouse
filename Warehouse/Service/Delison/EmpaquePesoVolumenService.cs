using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;
using Warehouse.Models.DTOs;

namespace Warehouse.Service.Delison
{
    public interface IEmpaquePesoVolumenService
    {
        Task<List<EmpaquePesoVolumenDelison>> GetByEmpaque(int idEmpaque);
        Task<List<EmpaquePesoVolumenDelison>> SaveByEmpaque(EmpaquePesoVolumenSaveDto dto);
        Task DeleteByEmpaqueIds(IEnumerable<int> idsEmpaque);
    }

    public class EmpaquePesoVolumenService : IEmpaquePesoVolumenService
    {
        private readonly DbWarehouseContext _context;

        public EmpaquePesoVolumenService(DbWarehouseContext context)
        {
            _context = context;
        }

        public async Task<List<EmpaquePesoVolumenDelison>> GetByEmpaque(int idEmpaque)
        {
            return await _context.EmpaquePesosVolumenes
                .Where(m => m.IdEmpaque == idEmpaque && m.Active)
                .OrderBy(m => m.Id)
                .ToListAsync();
        }

        /// <summary>Borra peso/volumen de varias presentaciones (al eliminar/reemplazar empaques).</summary>
        public async Task DeleteByEmpaqueIds(IEnumerable<int> idsEmpaque)
        {
            var ids = idsEmpaque?.ToList() ?? new List<int>();
            if (ids.Count == 0) return;
            var rows = await _context.EmpaquePesosVolumenes.Where(m => ids.Contains(m.IdEmpaque)).ToListAsync();
            if (rows.Count > 0) { _context.EmpaquePesosVolumenes.RemoveRange(rows); await _context.SaveChangesAsync(); }
        }

        /// <summary>Reemplaza TODO el peso/volumen de la presentación (borra e inserta; descarta filas vacías).</summary>
        public async Task<List<EmpaquePesoVolumenDelison>> SaveByEmpaque(EmpaquePesoVolumenSaveDto dto)
        {
            if (dto == null || dto.IdEmpaque <= 0)
                return new List<EmpaquePesoVolumenDelison>();

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var existing = await _context.EmpaquePesosVolumenes
                    .Where(m => m.IdEmpaque == dto.IdEmpaque)
                    .ToListAsync();
                if (existing.Count > 0)
                    _context.EmpaquePesosVolumenes.RemoveRange(existing);

                var items = (dto.Items ?? new List<EmpaquePesoVolumenItemDto>())
                    .Where(i => i.Medida.HasValue || i.IdUnidad.HasValue)
                    .Select(i => new EmpaquePesoVolumenDelison
                    {
                        IdEmpaque    = dto.IdEmpaque,
                        Medida       = i.Medida,
                        IdUnidad     = i.IdUnidad,
                        Active       = true,
                        DateModified = DateTime.Now
                    })
                    .ToList();

                if (items.Count > 0)
                    await _context.EmpaquePesosVolumenes.AddRangeAsync(items);

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
