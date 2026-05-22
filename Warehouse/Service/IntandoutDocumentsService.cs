using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse.Service
{
    public class IntandoutDocumentsService : IIntandoutDocumentsService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<IntandoutDocumentsService> _logger;

        public IntandoutDocumentsService(DbWarehouseContext context, ILogger<IntandoutDocumentsService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<IntandoutDocuments>> GetDocuments(int idDoc, string type)
        {
            try
            {
                var query = _context.IntandoutDocuments
                    .Where(d => d.IdDoc == idDoc && d.Type == type)
                    .OrderByDescending(d => d.Id)
                    .ToListAsync();
                return await query;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving intandout documents for idDoc {IdDoc} and type {Type}", idDoc, type);
                throw;
            }
        }

        public async Task<IntandoutDocuments?> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid intandout document ID provided: {Id}", id);
                    return null;
                }

                return await _context.IntandoutDocuments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving intandout document with ID {Id}", id);
                throw;
            }
        }

        public async Task<IntandoutDocuments> Save(IntandoutDocuments document)
        {
            try
            {
                _context.IntandoutDocuments.Add(document);
                await _context.SaveChangesAsync();
                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving intandout document");
                throw;
            }
        }

        public async Task<IntandoutDocuments?> Update(int id, IntandoutDocuments document)
        {
            var existingDocument = await _context.IntandoutDocuments.FindAsync(id);
            if (existingDocument == null)
            {
                _logger.LogWarning("Attempted to update non-existent intandout document with ID {Id}", id);
                return null;
            }

            try
            {
                existingDocument.IdDoc = document.IdDoc;
                existingDocument.DocumentName = document.DocumentName;
                existingDocument.UrlDocument = document.UrlDocument;
                existingDocument.Type = document.Type;

                await _context.SaveChangesAsync();
                return existingDocument;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating intandout document with ID {Id}", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating intandout document with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            var existingDocument = await _context.IntandoutDocuments.FindAsync(id);
            if (existingDocument == null)
            {
                _logger.LogWarning("Attempted to delete non-existent intandout document with ID {Id}", id);
                return false;
            }

            try
            {
                _context.IntandoutDocuments.Remove(existingDocument);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error deleting intandout document with ID {Id}", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting intandout document with ID {Id}", id);
                throw;
            }
        }
    }

    public interface IIntandoutDocumentsService
    {
        Task<List<IntandoutDocuments>> GetDocuments(int idDoc, string type);
        Task<IntandoutDocuments?> GetById(int id);
        Task<IntandoutDocuments> Save(IntandoutDocuments document);
        Task<IntandoutDocuments?> Update(int id, IntandoutDocuments document);
        Task<bool> Delete(int id);
    }
}
