using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse.Service
{
    public class InandoutEvidenceService : IInandoutEvidenceService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<InandoutEvidenceService> _logger;

        public InandoutEvidenceService(DbWarehouseContext context, ILogger<InandoutEvidenceService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<Inandoutevidence>> GetByInandout(int idInandout)
        {
            try
            {
                return await _context.Inandoutevidence
                    .Where(e => e.IdInandout == idInandout && e.Active)
                    .OrderByDescending(e => e.Id)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving evidence for inandout {IdInandout}", idInandout);
                throw;
            }
        }

        public async Task<Dictionary<int, int>> GetCountsByInandoutIds(IEnumerable<int> idInandoutList)
        {
            var ids = idInandoutList.Distinct().ToList();
            if (ids.Count == 0) return new Dictionary<int, int>();

            return await _context.Inandoutevidence
                .Where(e => e.Active && ids.Contains(e.IdInandout))
                .GroupBy(e => e.IdInandout)
                .Select(g => new { IdInandout = g.Key, Count = g.Count() })
                .AsNoTracking()
                .ToDictionaryAsync(x => x.IdInandout, x => x.Count);
        }

        public async Task<Inandoutevidence> Save(Inandoutevidence evidence)
        {
            try
            {
                evidence.CreatedAt = DateTime.UtcNow;
                evidence.Active = true;
                _context.Inandoutevidence.Add(evidence);
                await _context.SaveChangesAsync();
                return evidence;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving evidence for inandout {IdInandout}", evidence.IdInandout);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            var existing = await _context.Inandoutevidence.FindAsync(id);
            if (existing == null) return false;

            try
            {
                existing.Active = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting evidence {Id}", id);
                throw;
            }
        }

        public async Task<Inandoutevidence?> UpdateComment(int id, string? comment)
        {
            var existing = await _context.Inandoutevidence.FindAsync(id);
            if (existing == null) return null;

            try
            {
                existing.Comment = comment;
                await _context.SaveChangesAsync();
                return existing;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment for evidence {Id}", id);
                throw;
            }
        }
    }

    public interface IInandoutEvidenceService
    {
        Task<List<Inandoutevidence>> GetByInandout(int idInandout);
        Task<Dictionary<int, int>> GetCountsByInandoutIds(IEnumerable<int> idInandoutList);
        Task<Inandoutevidence> Save(Inandoutevidence evidence);
        Task<bool> Delete(int id);
        Task<Inandoutevidence?> UpdateComment(int id, string? comment);
    }
}
