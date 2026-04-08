using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse.Service;

public interface IItemCommentsService
{
    Task<List<ItemComment>> GetAsync(int idRequisicion, string numArticle);
    Task<ItemComment> AddAsync(ItemComment comment);
    Task<ItemComment?> UpdateAsync(int id, string text);
}

public class ItemCommentsService : IItemCommentsService
{
    private readonly DbWarehouseContext _context;

    public ItemCommentsService(DbWarehouseContext context)
    {
        _context = context;
    }

    public async Task<List<ItemComment>> GetAsync(int idRequisicion, string numArticle)
    {
        return await _context.ItemComments
            .Where(c => c.IdRequisicion == idRequisicion
                     && c.NumArticle == numArticle
                     && c.Active)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
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
}
