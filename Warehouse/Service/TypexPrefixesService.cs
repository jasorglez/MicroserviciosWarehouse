using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse.Service
{
    public class TypexPrefixesService : ITypexPrefixesService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<TypexPrefixesService> _logger;

        public TypexPrefixesService(DbWarehouseContext context, ILogger<TypexPrefixesService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<TypexPrefixes>> GetAll()
        {
            try
            {
                return await _context.TypexPrefixes
                    .Where(t => t.Active)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving TypexPrefixes.");
                throw;
            }
        }

        public async Task<TypexPrefixes?> GetByReqTypeAndId(string reqType, int idReqType)
        {
            try
            {
                return await _context.TypexPrefixes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.ReqType == reqType && t.IdReqType == idReqType && t.Active);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving TypexPrefixes with ReqType {ReqType} and IdReqType {IdReqType}.", reqType, idReqType);
                throw;
            }
        }

        public async Task Save(TypexPrefixes typexPrefixes)
        {
            try
            {
                // Verificar si ya existe un registro con el mismo ReqType e IdReqType
                var existing = await _context.TypexPrefixes
                    .FirstOrDefaultAsync(t => t.ReqType == typexPrefixes.ReqType && t.IdReqType == typexPrefixes.IdReqType);

                if (existing != null)
                {
                    throw new InvalidOperationException($"Ya existe un registro con ReqType '{typexPrefixes.ReqType}' e IdReqType '{typexPrefixes.IdReqType}'.");
                }

                _context.TypexPrefixes.Add(typexPrefixes);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving TypexPrefixes.");
                throw;
            }
        }

        public async Task<bool> Update(string reqType, int idReqType, TypexPrefixes typexPrefixes)
        {
            var existing = await _context.TypexPrefixes
                .FirstOrDefaultAsync(t => t.ReqType == reqType && t.IdReqType == idReqType && t.Active);

            if (existing == null)
            {
                return false;
            }

            try
            {
                existing.Prefix = typexPrefixes.Prefix;
                existing.Consecutive = typexPrefixes.Consecutive;
                existing.Active = typexPrefixes.Active;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating TypexPrefixes with ReqType {ReqType} and IdReqType {IdReqType}.", reqType, idReqType);
                throw;
            }
        }

        public async Task<bool> Delete(string reqType, int idReqType)
        {
            var existing = await _context.TypexPrefixes
                .FirstOrDefaultAsync(t => t.ReqType == reqType && t.IdReqType == idReqType && t.Active);

            if (existing == null)
            {
                _logger.LogWarning("Attempted to delete non-existent TypexPrefixes with ReqType {ReqType} and IdReqType {IdReqType}", reqType, idReqType);
                return false;
            }

            try
            {
                existing.Active = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting TypexPrefixes with ReqType {ReqType} and IdReqType {IdReqType}.", reqType, idReqType);
                throw;
            }
        }
    }

    public interface ITypexPrefixesService
    {
        Task<List<TypexPrefixes>> GetAll();
        Task<TypexPrefixes?> GetByReqTypeAndId(string reqType, int idReqType);
        Task Save(TypexPrefixes typexPrefixes);
        Task<bool> Update(string reqType, int idReqType, TypexPrefixes typexPrefixes);
        Task<bool> Delete(string reqType, int idReqType);
    }
}
