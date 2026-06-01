using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;

namespace Warehouse.Service.Delison
{
    public interface IGridColumnStateService
    {
        Task<GridColumnStateDelison?> Get(int idUser, string gridKey);
        Task<GridColumnStateDelison> Save(GridColumnStateDelison data);
    }

    public class GridColumnStateService : IGridColumnStateService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<GridColumnStateService> _logger;

        public GridColumnStateService(DbWarehouseContext context, ILogger<GridColumnStateService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        public async Task<GridColumnStateDelison?> Get(int idUser, string gridKey)
        {
            return await _context.GridColumnStates
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.IdUser == idUser && g.GridKey == gridKey);
        }

        // Upsert por (id_user, grid_key).
        public async Task<GridColumnStateDelison> Save(GridColumnStateDelison data)
        {
            var existing = await _context.GridColumnStates
                .FirstOrDefaultAsync(g => g.IdUser == data.IdUser && g.GridKey == data.GridKey);

            if (existing == null)
            {
                data.DateModified = DateTime.UtcNow;
                _context.GridColumnStates.Add(data);
                await _context.SaveChangesAsync();
                return data;
            }

            existing.ColumnState  = data.ColumnState;
            existing.DateModified = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existing;
        }
    }
}
