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

        public async Task<string> GetNextRequisitionNumber(int idBranch)
        {
            try
            {
                // Obtener el prefijo configurado para esta sucursal
                var prefixConfig = await _context.TypexPrefixes
                    .FirstOrDefaultAsync(t => t.ReqType == "branch" && t.IdReqType == idBranch && t.Active);

                if (prefixConfig == null)
                {
                    throw new InvalidOperationException($"No prefix configuration found for branch {idBranch}");
                }

                _logger.LogInformation($"DEBUG - Getting next number for branch {idBranch}, prefix: {prefixConfig.Prefix}");

                // Obtener la última requisición de esta sucursal
                var lastRequisition = await _context.Ocandreqs
                    .Where(o => o.IdReference == idBranch && o.Type == "REQUIS")
                    .OrderByDescending(o => o.Id)
                    .FirstOrDefaultAsync();

                var countReqs = await _context.Ocandreqs
                    .Where(o => o.IdReference == idBranch && o.Type == "REQUIS")
                    .CountAsync();

                _logger.LogInformation($"DEBUG - Found {countReqs} REQUIS documents for branch {idBranch}");

                int nextNumber = 1;

                if (lastRequisition != null && !string.IsNullOrEmpty(lastRequisition.Folio))
                {
                    _logger.LogInformation($"DEBUG - Last requisition folio: {lastRequisition.Folio}");

                    // Extraer el número del folio anterior
                    int lastNumber = ExtractNumberFromFolio(lastRequisition.Folio);

                    _logger.LogInformation($"DEBUG - Extracted number from folio: {lastNumber}");

                    if (lastNumber > 0)
                    {
                        nextNumber = lastNumber + 1;
                    }
                }
                else
                {
                    _logger.LogInformation($"DEBUG - No last requisition found, starting from 1");
                }

                _logger.LogInformation($"DEBUG - Next number to be assigned: {nextNumber}");

                // Actualizar el consecutivo en TypexPrefixes
                prefixConfig.Consecutive = nextNumber;
                await _context.SaveChangesAsync();

                // Generar y retornar el número completo
                return $"{prefixConfig.Prefix}-{nextNumber:D5}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating next requisition number for branch {IdBranch}.", idBranch);
                throw;
            }
        }

        private int ExtractNumberFromFolio(string folio)
        {
            try
            {
                if (string.IsNullOrEmpty(folio))
                    return 0;

                // Folio format: "BOD-00005" o "DELI-00001"
                // Separar por guion y tomar la segunda parte
                var parts = folio.Split('-');

                if (parts.Length >= 2)
                {
                    var numberPart = parts[parts.Length - 1];  // Última parte (los números)

                    if (int.TryParse(numberPart, out int number))
                    {
                        return number;
                    }
                }

                return 0;
            }
            catch
            {
                return 0;
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
        Task<string> GetNextRequisitionNumber(int idBranch);
    }
}
