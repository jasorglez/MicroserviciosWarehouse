
using Microsoft.EntityFrameworkCore;
using Warehouse.Models;
namespace Warehouse.Service
{
    public class InandoutService: IInandoutService
{
    private readonly DbWarehouseContext _context;
    private readonly ILogger<InandoutService> _logger;

    public InandoutService(DbWarehouseContext context, ILogger<InandoutService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<object>> GetInsAndOuts(int idProject, int IdWarehouse, string type)
    {
        try
        {
            return await _context.Inandouts
                .Where(i => i.IdProject == idProject && i.Type == type && IdWarehouse == i.IdWarehouse && i.Active == true)
                .Select(i => new
                {
                    i.Id,
                    i.IdProject,
                    i.IdWarehouse,
                    i.IdType,
                    i.Folio,
                    i.Date,
                    i.DeliveryDate,
                    i.IdOc,
                    i.NumBill,i.IdOt,
                    i.DeliverName,
                    i.CountRow, 
                    i.Comment,
                    i.Type,
                    i.Active
                })
                .AsNoTracking()
                .ToListAsync<object>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ins and outs");
            throw;
        }
    }

    public async Task<object?> GetInAndOutById(int id)
    {
        try
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid Order ID provided: {Id}", id);
                return null;
            }
            return await _context.Inandouts
                .Where(i => i.Id == id && i.Active == true)
                .Select(i => new
                {
                    i.Id,
                    i.IdProject,
                    i.IdWarehouse,
                    i.IdType,
                    i.Folio,
                    i.Date,
                    i.IdOt,
                    i.DeliveryDate,
                    i.IdOc,
                    i.NumBill,
                    i.DeliverName,
                    i.CountRow,
                    i.Comment,
                    i.Type,
                    i.Active
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ins and outs");
            throw;
        }
    }

    public async Task Save(Inandout inandout)
    {
        try
        {
            _context.Inandouts.Add(inandout);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving inandout");
            throw;
        }
    }

        public async Task<Inandout?> Update(int id, Inandout inandout)
        {
            var existingItem = await _context.Inandouts.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Invalid Order ID provided: {Id}", id);
                return null;
            }

            try
            {
                
                if (inandout.IdType != 0)
                    existingItem.IdType = inandout.IdType;

                if (inandout.Date.HasValue)
                    existingItem.Date = inandout.Date;

                if (inandout.DeliveryDate.HasValue)
                    existingItem.DeliveryDate = inandout.DeliveryDate;

                if (inandout.IdOc.HasValue)
                    existingItem.IdOc = inandout.IdOc;

                if (inandout.NumBill != null)
                    existingItem.NumBill = inandout.NumBill;

                if (inandout.DeliverName != null)
                    existingItem.DeliverName = inandout.DeliverName;

                if (inandout.CountRow.HasValue)
                    existingItem.CountRow = inandout.CountRow;

                if (inandout.Comment != null)
                    existingItem.Comment = inandout.Comment; 

                await _context.SaveChangesAsync();
                return existingItem;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error updating inandout");
                throw;
            }
        }

        public async Task<bool> Delete(int id)
    {
        var existingItem = await _context.Inandouts.FindAsync(id);
        if (existingItem == null)
        {
            _logger.LogWarning("Invalid Order ID provided: {Id}", id);
            return false;
        }

        try
        {
            existingItem.Active = false;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting inandout with ID {id}", id);
            throw;
        }
    }
}

public interface IInandoutService
{
    Task<List<object>> GetInsAndOuts(int idProject, int IdWarehouse, string type);
    Task<object?> GetInAndOutById(int id);
    Task Save(Inandout inandout);
    Task<Inandout?> Update(int id, Inandout inandout);
    Task<bool> Delete(int id);
}
}