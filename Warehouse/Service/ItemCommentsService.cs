using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse.Service;

public interface IItemCommentsService
{
    Task<List<ItemComment>> GetAsync(string documentType, int idDocument, string numArticle, int? idProvider = null);
    Task<ItemComment> AddAsync(ItemComment comment);
    Task<ItemComment?> UpdateAsync(int id, string text);
    Task<DocumentCommentFlags> GetFlagsAsync(string documentType, int idDocument);
    Task<int> DeleteByArticleAsync(string documentType, int idDocument, string numArticle, string? textPrefix = null);
    Task<int> CountAsync(string documentType, int idDocument);
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

    // Borrado FÍSICO de comentarios de un artículo dentro de un documento.
    // - Sin textPrefix: borra TODOS los comentarios del artículo. Se usa al eliminar un renglón
    //   de la requisición (los comentarios se atan a documentType+idDocument+numArticle, NO al id
    //   del renglón, así que al re-agregar el mismo artículo no deben reaparecer).
    // - Con textPrefix: borra solo los que empiezan con ese prefijo (ej. "🧮 [Req]" = comentarios
    //   del panel de presentaciones), para REEMPLAZAR el anterior y dejar solo el último, sin tocar
    //   los comentarios manuales del usuario.
    public async Task<int> DeleteByArticleAsync(string documentType, int idDocument, string numArticle, string? textPrefix = null)
    {
        if (string.IsNullOrEmpty(numArticle)) return 0;
        var query = _context.ItemComments
            .Where(c => c.DocumentType == documentType
                     && c.IdDocument == idDocument
                     && c.NumArticle == numArticle);
        if (!string.IsNullOrEmpty(textPrefix))
            query = query.Where(c => c.Text.StartsWith(textPrefix));
        var rows = await query.ToListAsync();
        if (rows.Count == 0) return 0;
        _context.ItemComments.RemoveRange(rows);
        await _context.SaveChangesAsync();
        return rows.Count;
    }

    public async Task<int> CountAsync(string documentType, int idDocument)
    {
        return await _context.ItemComments
            .CountAsync(c => c.DocumentType == documentType && c.IdDocument == idDocument && c.Active);
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
