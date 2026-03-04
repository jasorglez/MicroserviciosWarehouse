
using Microsoft.EntityFrameworkCore;
using Warehouse.Models;
using System.Threading.Tasks;

namespace Warehouse.Service
{
    public class SubfamilyxProviderService : ISubfamilyxProviderService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<SubfamilyxProviderService> _logger;

        public SubfamilyxProviderService(DbWarehouseContext context, ILogger<SubfamilyxProviderService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Obtener todos los registros activos por IdProvider
        public async Task<List<SubfamilyxProvider>> GetByProvider(int idProvider)
        {
            return await _context.SubfamilyxProviders
                .Where(s => s.IdProvider == idProvider && s.Active)
                .OrderBy(s => s.Id)
                .ToListAsync();
        }

        // Obtener vigentes por IdProvider y sub-family
        public async Task<List<object>> GetSubfamiliesByProvider(int idProvider)
        {
            try
            {
                var result = await (
                    from p in _context.SubfamilyxProviders
                    join c in _context.Catalogs
                        on p.IdSubfamily equals c.Id
                    where p.IdProvider == idProvider
                          && p.Active == true
                          && p.Vigente == true
                          && c.Type == "SUB-FAM"
                    orderby p.Vigente
                    select new
                    {
                        p.Id,
                        p.IdProvider,
                        p.IdSubfamily,
                        p.Active,
                        p.Vigente,
                        Description = c.Description
                    }
                ).ToListAsync();

                return result.Cast<object>().ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Obtener un registro por Id
        public async Task<SubfamilyxProvider?> GetById(int id)
        {
            return await _context.SubfamilyxProviders
                .FirstOrDefaultAsync(s => s.Id == id && s.Active);
        }

        // Crear un nuevo registro
        public async Task<SubfamilyxProvider> Create(SubfamilyxProvider subfamilyxProvider)
        {
            // Validar: no se puede marcar como principal si no está vigente
            if (subfamilyxProvider.Principal && !subfamilyxProvider.Vigente)
            {
                _logger.LogWarning("Cannot set Principal=true when Vigente=false for IdProvider={IdProvider}",
                    subfamilyxProvider.IdProvider);
                subfamilyxProvider.Principal = false;
            }

            // Si se marca como principal, desmarcar todos los demás del mismo proveedor
            if (subfamilyxProvider.Principal)
            {
                await UnsetOtherPrincipals(subfamilyxProvider.IdProvider, null);
            }

            _context.SubfamilyxProviders.Add(subfamilyxProvider);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created SubfamilyxProvider: IdSubfamily={IdSubfamily}, IdProvider={IdProvider}",
                subfamilyxProvider.IdSubfamily, subfamilyxProvider.IdProvider);

            return subfamilyxProvider;
        }

        // Actualizar un registro existente
        public async Task<SubfamilyxProvider?> Update(int id, SubfamilyxProvider subfamilyxProvider)
        {
            var existing = await _context.SubfamilyxProviders.FindAsync(id);
            if (existing == null || !existing.Active)
            {
                _logger.LogWarning("SubfamilyxProvider with Id={Id} not found or not active", id);
                return null;
            }

            // Validar: no se puede marcar como principal si no está vigente
            if (subfamilyxProvider.Principal && !subfamilyxProvider.Vigente)
            {
                _logger.LogWarning("Cannot set Principal=true when Vigente=false for Id={Id}", id);
                subfamilyxProvider.Principal = false;
            }

            // Si se desmarca como vigente, también desmarcar como principal
            if (!subfamilyxProvider.Vigente && existing.Principal)
            {
                _logger.LogInformation("Unsetting Principal because Vigente=false for Id={Id}", id);
                subfamilyxProvider.Principal = false;

                // Buscar y marcar otro registro activo como principal
                var otherActiveRecord = await _context.SubfamilyxProviders
                    .Where(s => s.IdProvider == existing.IdProvider && s.Id != id && s.Vigente && s.Active)
                    .FirstOrDefaultAsync();

                if (otherActiveRecord != null)
                {
                    otherActiveRecord.Principal = true;
                    _logger.LogInformation("Set Principal=true for Id={OtherId} as replacement", otherActiveRecord.Id);
                }
            }

            // Si se intenta desmarcar como principal, validar que haya al menos otro activo
            if (!subfamilyxProvider.Principal && existing.Principal)
            {
                var otherActivePrincipals = await _context.SubfamilyxProviders
                    .Where(s => s.IdProvider == existing.IdProvider && s.Id != id && s.Vigente && s.Active)
                    .CountAsync();

                if (otherActivePrincipals == 0)
                {
                    _logger.LogWarning("Cannot unset Principal for Id={Id} - no other active records exist", id);
                    subfamilyxProvider.Principal = true; // Forzar a mantenerlo como principal
                }
            }

            // Si se marca como principal, desmarcar todos los demás del mismo proveedor
            if (subfamilyxProvider.Principal && !existing.Principal)
            {
                await UnsetOtherPrincipals(existing.IdProvider, id);
            }

            // Actualizar propiedades
            existing.IdSubfamily = subfamilyxProvider.IdSubfamily;
            existing.Vigente = subfamilyxProvider.Vigente;
            existing.Principal = subfamilyxProvider.Principal;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated SubfamilyxProvider: Id={Id}, IdSubfamily={IdSubfamily}, Vigente={Vigente}, Principal={Principal}",
                id, subfamilyxProvider.IdSubfamily, subfamilyxProvider.Vigente, subfamilyxProvider.Principal);

            return existing;
        }

        // Marcar como inactivo (soft delete)
        public async Task<bool> Delete(int idSubfamily, int idProvider)
        {
            var records = await _context.SubfamilyxProviders
                .Where(s => s.IdSubfamily == idSubfamily && s.IdProvider == idProvider && s.Active)
                .ToListAsync();

            if (!records.Any())
            {
                _logger.LogWarning("No active SubfamilyxProvider found with IdSubfamily={IdSubfamily} and IdProvider={IdProvider}",
                    idSubfamily, idProvider);
                return false;
            }

            foreach (var record in records)
            {
                record.Active = false;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Soft deleted SubfamilyxProvider: IdSubfamily={IdSubfamily}, IdProvider={IdProvider}",
                idSubfamily, idProvider);

            return true;
        }

        // Marcar como inactivo por Id (soft delete)
        public async Task<bool> DeleteById(int id)
        {
            var record = await _context.SubfamilyxProviders.FindAsync(id);
            if (record == null || !record.Active)
            {
                _logger.LogWarning("SubfamilyxProvider with Id={Id} not found or already inactive", id);
                return false;
            }

            // Si el registro a eliminar es principal, buscar otro activo y marcarlo como principal
            if (record.Principal)
            {
                var otherActiveRecord = await _context.SubfamilyxProviders
                    .Where(s => s.IdProvider == record.IdProvider && s.Id != id && s.Vigente && s.Active)
                    .FirstOrDefaultAsync();

                if (otherActiveRecord != null)
                {
                    otherActiveRecord.Principal = true;
                    _logger.LogInformation("Set Principal=true for Id={OtherId} as replacement after delete", otherActiveRecord.Id);
                }
            }

            record.Active = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Soft deleted SubfamilyxProvider by Id: Id={Id}", id);

            return true;
        }

        // Alternar estado Active
        public async Task<bool> ToggleActive(int id)
        {
            var record = await _context.SubfamilyxProviders.FindAsync(id);
            if (record == null) return false;

            record.Active = !record.Active;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Toggled Active for SubfamilyxProvider: Id={Id}, NewActive={Active}",
                id, record.Active);

            return true;
        }

        // Método auxiliar para desmarcar otros registros como principales
        private async Task UnsetOtherPrincipals(int idProvider, int? excludeId)
        {
            var otherPrincipals = await _context.SubfamilyxProviders
                .Where(s => s.IdProvider == idProvider && s.Principal && s.Active)
                .ToListAsync();

            if (excludeId.HasValue)
            {
                otherPrincipals = otherPrincipals.Where(s => s.Id != excludeId.Value).ToList();
            }

            foreach (var record in otherPrincipals)
            {
                record.Principal = false;
            }

            if (otherPrincipals.Any())
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Unset Principal for {Count} records of IdProvider={IdProvider}",
                    otherPrincipals.Count, idProvider);
            }
        }

        // Verificar si existe un registro activo
        public async Task<bool> Exists(int idSubfamily, int idProvider)
        {
            return await _context.SubfamilyxProviders
                .AnyAsync(s => s.IdSubfamily == idSubfamily && s.IdProvider == idProvider && s.Active);
        }

        // Obtener el registro principal de un proveedor
        public async Task<SubfamilyxProvider?> GetPrincipalByProvider(int idProvider)
        {
            return await _context.SubfamilyxProviders
                .FirstOrDefaultAsync(s => s.IdProvider == idProvider && s.Principal && s.Active);
        }
    }

    public interface ISubfamilyxProviderService
    {
        Task<List<object>> GetSubfamiliesByProvider(int idProvider);
        Task<List<SubfamilyxProvider>> GetByProvider(int idProvider);
        Task<SubfamilyxProvider?> GetById(int id);
        Task<SubfamilyxProvider> Create(SubfamilyxProvider subfamilyxProvider);
        Task<SubfamilyxProvider?> Update(int id, SubfamilyxProvider subfamilyxProvider);
        Task<bool> Delete(int idSubfamily, int idProvider);
        Task<bool> DeleteById(int id);
        Task<bool> ToggleActive(int id);
        Task<bool> Exists(int idSubfamily, int idProvider);
        Task<SubfamilyxProvider?> GetPrincipalByProvider(int idProvider);
    }
}
