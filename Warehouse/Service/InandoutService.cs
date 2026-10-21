
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

    public async Task<List<object>> GetInsAndOuts(int? idProject, int idWarehouse, string type)
    {
        try
        {
            var query = _context.Inandouts
                .Where(i => i.Type == type
                    && (i.IdWarehouse == idWarehouse || (type == "IN" && i.IdWarehouseOrigin == idWarehouse))
                    && i.Active == true);

            if (idProject.HasValue && idProject.Value > 0)
                query = query.Where(i => i.IdProject == idProject.Value);

            return await query
                .Select(i => new
                {
                    i.Id,
                    i.IdProject,
                    i.IdWarehouse,
                    i.IdWarehouseOrigin,
                    i.IdType,
                    i.Folio,
                    i.Date,
                    i.DeliveryDate,
                    i.IdOc,
                    i.IdEntry,
                    i.NumBill,i.IdOt,
                    i.DeliverName, i.IdAutoriza,
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
                    i.IdWarehouseOrigin,
                    i.IdType,
                    i.Folio,
                    i.Date,
                    i.IdOt,
                    i.DeliveryDate,
                    i.IdOc,
                    i.IdEntry,
                    i.NumBill,
                    i.DeliverName,
                    i.CountRow,
                    i.Comment,
                    i.Type, i.IdAutoriza,
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
            var movementDescription = inandout.IdType.HasValue
                ? await _context.Catalogs.AsNoTracking()
                    .Where(catalog => catalog.Id == inandout.IdType.Value)
                    .Select(catalog => catalog.Description)
                    .FirstOrDefaultAsync()
                : null;
            var isTransfer = inandout.Type == "IN" &&
                string.Equals(movementDescription?.Trim(), "TRASPASO", StringComparison.OrdinalIgnoreCase);

            if (isTransfer && (!inandout.IdWarehouseOrigin.HasValue || inandout.IdWarehouseOrigin == inandout.IdWarehouse))
                throw new InvalidOperationException("Selecciona un almacén de origen distinto al almacén de destino.");

            if (!isTransfer)
            {
                await ValidateEntrySource(inandout);
                _context.Inandouts.Add(inandout);
                await _context.SaveChangesAsync();
                return;
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            _context.Inandouts.Add(inandout);
            await _context.SaveChangesAsync();

            var outboundTransferTypeId = await _context.Catalogs.AsNoTracking()
                .Where(catalog => catalog.Type == "OUT" && catalog.Active == 1 && catalog.Description == "TRASPASO")
                .Select(catalog => (int?)catalog.Id)
                .FirstOrDefaultAsync();
            var pairedMovement = new Inandout
            {
                IdProject = inandout.IdProject,
                IdWarehouse = inandout.IdWarehouseOrigin,
                IdWarehouseOrigin = inandout.IdWarehouse,
                IdType = outboundTransferTypeId ?? inandout.IdType,
                IdOt = inandout.IdOt,
                IdAutoriza = inandout.IdAutoriza,
                Date = inandout.Date,
                DeliveryDate = inandout.DeliveryDate,
                IdOc = 0,
                Folio = $"T-{inandout.Id}",
                NumBill = null,
                DeliverName = inandout.DeliverName,
                CountRow = inandout.CountRow,
                Comment = $"Salida por traspaso a almacén {inandout.IdWarehouse}",
                Type = "OUT",
                Active = inandout.Active,
                IdTransferPair = inandout.Id
            };
            _context.Inandouts.Add(pairedMovement);
            await _context.SaveChangesAsync();
            inandout.IdTransferPair = pairedMovement.Id;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
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
                await ValidateEntrySource(inandout);
                if (existingItem.IdTransferPair.HasValue && inandout.Type == "IN")
                {
                    var updatedDescription = inandout.IdType.HasValue
                        ? await _context.Catalogs.AsNoTracking()
                            .Where(catalog => catalog.Id == inandout.IdType.Value)
                            .Select(catalog => catalog.Description)
                            .FirstOrDefaultAsync()
                        : null;
                    if (!string.Equals(updatedDescription?.Trim(), "TRASPASO", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("No se puede cambiar el tipo de un traspaso ya creado.");
                    if (!inandout.IdWarehouseOrigin.HasValue || inandout.IdWarehouseOrigin == inandout.IdWarehouse)
                        throw new InvalidOperationException("Selecciona un almacén de origen distinto al almacén de destino.");
                }

                if (inandout.Folio != null)
                    existingItem.Folio = inandout.Folio;

                if (inandout.IdType != null && inandout.IdType != 0)
                    existingItem.IdType = inandout.IdType;

                existingItem.IdWarehouseOrigin = inandout.IdWarehouseOrigin;

                if (existingItem.IdTransferPair.HasValue)
                {
                    var pairedMovement = await _context.Inandouts.FindAsync(existingItem.IdTransferPair.Value);
                    if (pairedMovement != null && existingItem.Type == "IN")
                    {
                        pairedMovement.IdWarehouse = inandout.IdWarehouseOrigin;
                        pairedMovement.IdWarehouseOrigin = inandout.IdWarehouse;
                        pairedMovement.Date = inandout.Date;
                        pairedMovement.DeliveryDate = inandout.DeliveryDate;
                        pairedMovement.IdOt = inandout.IdOt;
                        pairedMovement.IdAutoriza = inandout.IdAutoriza;
                        pairedMovement.DeliverName = inandout.DeliverName;
                        pairedMovement.Comment = $"Salida por traspaso a almacén {inandout.IdWarehouse}";
                    }
                }

                if (inandout.Date.HasValue)
                    existingItem.Date = inandout.Date;

                if (inandout.DeliveryDate.HasValue)
                    existingItem.DeliveryDate = inandout.DeliveryDate;

                // IdOt: permite null (sin OT) o cualquier valor
                existingItem.IdOt = inandout.IdOt;

                // IdOc: permite 0 (sin OC) o cualquier valor
                existingItem.IdOc = inandout.IdOc;
                existingItem.IdEntry = inandout.IdEntry;

                if (inandout.NumBill != null)
                    existingItem.NumBill = inandout.NumBill;

                if (inandout.DeliverName != null)
                    existingItem.DeliverName = inandout.DeliverName;

                if (inandout.CountRow.HasValue)
                    existingItem.CountRow = inandout.CountRow;

                if (inandout.IdAutoriza.HasValue)
                    existingItem.IdAutoriza = inandout.IdAutoriza;

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
            var hasDetails = await _context.Detailsinandout.AnyAsync(d => d.IdInandout == id && d.Active);
            var hasOutgoingMovements = existingItem.Type == "IN" && await _context.Inandouts.AnyAsync(m =>
                m.Active == true && m.Type == "OUT" && m.IdEntry == id);
            var pairedHasDetails = existingItem.IdTransferPair.HasValue && await _context.Detailsinandout.AnyAsync(d =>
                d.IdInandout == existingItem.IdTransferPair.Value && d.Active);
            if (hasDetails || hasOutgoingMovements || pairedHasDetails)
                throw new InvalidOperationException("No se puede eliminar el vale porque contiene partidas o tiene Salidas relacionadas. Elimina primero sus partidas y movimientos relacionados.");

            existingItem.Active = false;
            if (existingItem.IdTransferPair.HasValue)
            {
                var pairedMovement = await _context.Inandouts.FindAsync(existingItem.IdTransferPair.Value);
                if (pairedMovement != null) pairedMovement.Active = false;
            }
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting inandout with ID {id}", id);
            throw;
        }
    }

    private async Task ValidateEntrySource(Inandout movement)
    {
        if (movement.Type != "OUT" || !movement.IdEntry.HasValue || movement.IdEntry.Value <= 0) return;
        var source = await _context.Inandouts.AsNoTracking().FirstOrDefaultAsync(item =>
            item.Id == movement.IdEntry.Value && item.Active == true && item.Type == "IN");
        if (source == null)
            throw new InvalidOperationException("El vale de Entrada seleccionado no existe o ya no está activo.");
        if (source.IdProject != movement.IdProject || source.IdWarehouse != movement.IdWarehouse)
            throw new InvalidOperationException("El vale de Entrada debe pertenecer al mismo proyecto y almacén de la Salida.");
    }
}

public interface IInandoutService
{
    Task<List<object>> GetInsAndOuts(int? idProject, int idWarehouse, string type);
    Task<object?> GetInAndOutById(int id);
    Task Save(Inandout inandout);
    Task<Inandout?> Update(int id, Inandout inandout);
    Task<bool> Delete(int id);
}
}
