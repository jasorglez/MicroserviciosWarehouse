using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse.Service;

public interface IItemCommentsService
{
    Task<List<ItemComment>> GetAsync(string documentType, int idDocument, string numArticle, int? idProvider = null);
    Task<ItemComment> AddAsync(ItemComment comment);
    Task<ItemComment?> UpdateAsync(int id, string text);
    Task<DocumentCommentFlags> GetFlagsAsync(string documentType, int idDocument);
}

public class DocumentCommentFlags
{
    public bool HasNoAuth    { get; set; }
    public bool HasChangeSpec { get; set; }
}

public class ItemCommentsService : IItemCommentsService
{
    private readonly DbWarehouseContext _context;

    public ItemCommentsService(DbWarehouseContext context)
    {
        _context = context;
    }

    public async Task<List<ItemComment>> GetAsync(string documentType, int idDocument, string numArticle, int? idProvider = null)
    {
        var query = _context.ItemComments
            .Where(c => c.DocumentType == documentType
                     && c.IdDocument == idDocument
                     && c.Active);

        if (idProvider.HasValue)
        {
            query = query.Where(c => c.IdProvider == idProvider.Value);
            if (!string.IsNullOrEmpty(numArticle))
                query = query.Where(c => c.NumArticle == numArticle);
        }
        else
            query = query.Where(c => c.NumArticle == numArticle && c.IdProvider == null);

        return await query.OrderBy(c => c.CreatedAt).ToListAsync();
    }

    public async Task<ItemComment> AddAsync(ItemComment comment)
    {
        comment.CreatedAt = DateTime.UtcNow;
        _context.ItemComments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task<ItemComment?> UpdateAsync(int id, string text)
    {
        var comment = await _context.ItemComments.FindAsync(id);
        if (comment == null) return null;
        comment.Text = text;
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task<DocumentCommentFlags> GetFlagsAsync(string documentType, int idDocument)
    {
        var texts = await _context.ItemComments
            .Where(c => c.DocumentType == documentType && c.IdDocument == idDocument && c.Active)
            .Select(c => c.Text)
            .ToListAsync();

        bool hasNoAuth    = texts.Any(t => t != null && t.StartsWith("COMPRA NO AUTORIZADA"));
        bool hasChangeSpec = texts.Any(t => t != null && t.StartsWith("CAMBIO DE ESPECIFICACIONES"));

        return new DocumentCommentFlags { HasNoAuth = hasNoAuth, HasChangeSpec = hasChangeSpec };
    }
}
