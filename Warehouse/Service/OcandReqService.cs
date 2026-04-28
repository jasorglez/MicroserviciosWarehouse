
using Microsoft.EntityFrameworkCore;
using Warehouse.Models;
using Warehouse.Models.DTOs;

namespace Warehouse.Service
{
    public class OcandreqService : IOcandreqService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<OcandreqService> _logger;

        public OcandreqService(DbWarehouseContext context, ILogger<OcandreqService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<object>> GetOcReq( int idRoot, string type)
        {
            try
            {
                var result = await _context.Ocandreqs
                    .Where(o =>
                        o.Active == true &&
                        o.IdRoot == idRoot &&
                        o.Type == type)
                    .OrderByDescending(o => o.DateModified)
                    .Select(o => new
                    {
                        o.Id,
                        o.IdRoot,
                        o.Folio,
                        o.TypeReference,
                        o.IdReference,
                        o.IdReq,
                        o.DateCreate,
                        o.DateModified,
                        o.IdDepartament,
                        o.Delivery,
                        o.DeliveryTime,
                        o.TypeOc,
                        o.DateSupply,
                        o.IdPayment,
                        o.IdCurrency,
                        o.Conditions,
                        o.IdAuthorize,
                        o.IdSolicit,
                        o.IdProvider,
                        o.Solicit,
                        o.Priority,
                        o.Type,
                        o.Pedimento,
                        o.Comments,
                        o.CompliancePedimento,
                        o.ComplianceRequesicion,
                        o.IvaRetention,
                        o.Address,
                        o.City,
                        o.Phone,
                        o.Discount,
                        o.Close,
                        o.CountItem,
                        o.Locked,
                        o.Active,
                        o.AuthorizeName,
                        o.AuthorizationStatus,
                        o.RejectionReason,
                        o.AuthorizedAt,
                        countrow = _context.Detailsreqoc
                            .Count(d => d.IdMovement == o.Id && d.Active == true)
                    })
                    .AsNoTracking()
                    .ToListAsync();

                return result.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Orders");
                throw;
            }
        }


        public async Task<List<object>> GetOrders(string typeReference, int idReference, string type)
        {
            try
            {
                var result = await _context.Ocandreqs
                    .Where(o =>
                        o.Active == true &&
                        o.TypeReference == typeReference &&
                        o.IdReference == idReference &&
                        o.Type == type)
                    .OrderByDescending(o => o.DateModified)
                    .Select(o => new
                    {
                        o.Id,
                        o.Folio,
                        o.TypeReference,
                        o.IdReference,
                        o.IdReq,
                        o.DateCreate,
                        o.DateModified,
                        o.IdDepartament,
                        o.Delivery,
                        o.DeliveryTime,
                        o.TypeOc,
                        o.DateSupply,
                        o.IdPayment,
                        o.IdCurrency,
                        o.Conditions,
                        o.IdAuthorize,
                        o.IdSolicit,
                        o.IdProvider,
                        o.Solicit,
                        o.Priority,
                        o.Type,
                        o.Pedimento,
                        o.Comments,
                        o.CompliancePedimento,
                        o.ComplianceRequesicion,
                        o.IvaRetention,
                        o.Address,
                        o.City,
                        o.Phone,
                        o.Discount,
                        o.Close,
                        o.CountItem,
                        o.Locked,
                        o.Active,
                        o.AuthorizeName,
                        o.AuthorizationStatus,
                        o.RejectionReason,
                        o.AuthorizedAt,
                        countrow = _context.Detailsreqoc
                            .Count(d => d.IdMovement == o.Id && d.Active == true)
                    })
                    .AsNoTracking()
                    .ToListAsync();

                return result.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Orders");
                throw;
            }
        }


        public async Task<object?> GetOrderById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid Order ID provided: {Id}", id);
                    return null;
                }

                return await _context.Ocandreqs
                    .Where(o => o.Active == true && o.Id == id)
                    .Select(o => new
                    {
                        o.Id,
                        o.Folio,
                        o.TypeReference,
                        o.IdReference,
                        o.IdReq,
                        o.DateCreate,
                        o.DateModified,
                        o.IdDepartament,
                        o.Delivery,
                        o.DeliveryTime,
                        o.TypeOc,
                        o.DateSupply,
                        o.IdPayment,
                        o.IdCurrency,
                        o.Conditions,
                        o.IdAuthorize,
                        o.IdSolicit,
                        o.IdProvider,
                        o.Solicit,
                        o.Priority,
                        o.Type,
                        o.Pedimento,
                        o.CompliancePedimento,
                        o.ComplianceRequesicion,
                        o.Comments,
                        o.IvaRetention,
                        o.Address,
                        o.City,
                        o.Phone,
                        o.Discount,
                        o.Close,
                        o.CountItem,
                        o.Locked,
                        o.Active,
                        o.AuthorizeName,
                        o.AuthorizationStatus,
                        o.RejectionReason,
                        o.AuthorizedAt
                    })
            .AsNoTracking()
            .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Order by ID {Id}", id);
                throw;
            }
        }

        public async Task Save(Ocandreq ocandreq)
        {
            try
            {
                _context.Ocandreqs.Add(ocandreq);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Order");
                throw;
            }
        }

        public async Task<Ocandreq?> Update(int id, Ocandreq ocandreq)
        {
            var existingItem = await _context.Ocandreqs.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to update non-existent Order with ID {Id}", id);
                return null;
            }

            try
            {
                _context.Entry(existingItem).CurrentValues.SetValues(ocandreq);
                await _context.SaveChangesAsync();
                return existingItem;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating Order with ID {Id}", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Order with ID {Id}", id);
                throw;
            }
        }

        public async Task<Ocandreq?> UpdateAuthorization(int id, AuthorizationCallbackDto dto)
        {
            var existingItem = await _context.Ocandreqs.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to update authorization for non-existent Order with ID {Id}", id);
                return null;
            }

            try
            {
                existingItem.IdAuthorize = dto.IdAuthorize;
                existingItem.AuthorizeName = dto.AuthorizeName;
                existingItem.AuthorizationStatus = dto.Status == "APPROVED" ? "Autorizado" : "Rechazado";
                existingItem.RejectionReason = dto.RejectionReason;
                existingItem.AuthorizedAt = dto.RespondedAt;

                await _context.SaveChangesAsync();
                return existingItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating authorization for Order with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            var existingItem = await _context.Ocandreqs.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to delete non-existent Order with ID {Id}", id);
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
                _logger.LogError(ex, "Error deleting Order with ID {Id}", id);
                throw;
            }
        }

        public async Task<Ocandreq?> SetLocked(int id, bool locked)
        {
            var existingItem = await _context.Ocandreqs.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to lock non-existent Order with ID {Id}", id);
                return null;
            }

            try
            {
                existingItem.Locked = locked;
                await _context.SaveChangesAsync();
                return existingItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting locked status for Order with ID {Id}", id);
                throw;
            }
        }

        public async Task<Ocandreq?> SetCountItem(int id, int countItem)
        {
            var existingItem = await _context.Ocandreqs.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to update countItem for non-existent Order with ID {Id}", id);
                return null;
            }

            try
            {
                existingItem.CountItem = countItem;
                await _context.SaveChangesAsync();
                return existingItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting countItem for Order with ID {Id}", id);
                throw;
            }
        }

        public async Task<Ocandreq?> SetTotal(int id, decimal total)
        {
            var existingItem = await _context.Ocandreqs.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to update total for non-existent Order with ID {Id}", id);
                return null;
            }

            try
            {
                existingItem.Total = total;
                await _context.SaveChangesAsync();
                return existingItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting total for Order with ID {Id}", id);
                throw;
            }
        }

        public async Task<List<ReqTypeOcFlagDto>> GetTypeOcFlags(List<int> reqIds)
        {
            if (reqIds == null || !reqIds.Any()) return new List<ReqTypeOcFlagDto>();

            var cotizMap = await _context.Ocandreqs
                .Where(c => c.Active == true && c.Type == "COTIZ" && c.IdReq != null && reqIds.Contains(c.IdReq!.Value))
                .Select(c => new { CotizId = c.Id, ReqId = c.IdReq!.Value })
                .ToListAsync();

            if (!cotizMap.Any()) return new List<ReqTypeOcFlagDto>();

            var cotizIdList   = cotizMap.Select(x => x.CotizId).ToList();
            var cotizReqMap   = cotizMap.ToDictionary(x => x.CotizId, x => x.ReqId);

            var details = await _context.Detailsreqoc
                .Where(d => d.Active == true && d.TypeOc != null && cotizIdList.Contains(d.IdMovement))
                .Select(d => new { d.IdMovement, d.TypeOc })
                .ToListAsync();

            var result = details
                .GroupBy(d => cotizReqMap[d.IdMovement])
                .Select(g => new ReqTypeOcFlagDto
                {
                    ReqId         = g.Key,
                    HasNoAuth     = g.Any(d => d.TypeOc == "COMPRA NO AUTORIZADA"),
                    HasChangeSpec = g.Any(d => d.TypeOc == "CAMBIO DE ESPECIFICACIONES")
                })
                .ToList();

            return result;
        }

        public async Task<object> GetComparisonData(int pedimentoId)
        {
            try
            {
                // Obtener el pedimento
                var pedimento = await _context.Ocandreqs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == pedimentoId && o.Active == true);

                if (pedimento == null)
                {
                    return new { error = "Pedimento no encontrado" };
                }

                // Obtener los 3 proveedores del pedimento (slots A/B/C)
                var proveedorCotizs = await _context.Ocandreqs
                    .Where(o => o.TypeReference == "delison" &&
                                o.IdReference == pedimentoId &&
                                o.Type == "COTIZ" &&
                                o.Active == true)
                    .AsNoTracking()
                    .ToListAsync();

                var slotA = proveedorCotizs.FirstOrDefault(c => c.Folio.Contains("-A-"));
                var slotB = proveedorCotizs.FirstOrDefault(c => c.Folio.Contains("-B-"));
                var slotC = proveedorCotizs.FirstOrDefault(c => c.Folio.Contains("-C-"));

                var proveedores = new List<object>();
                if (slotA != null && slotA.IdProvider.HasValue && slotA.IdProvider > 0)
                    proveedores.Add(new { id = slotA.IdProvider.Value, nombre = slotA.Solicit ?? $"Proveedor {slotA.IdProvider}" });
                if (slotB != null && slotB.IdProvider.HasValue && slotB.IdProvider > 0)
                    proveedores.Add(new { id = slotB.IdProvider.Value, nombre = slotB.Solicit ?? $"Proveedor {slotB.IdProvider}" });
                if (slotC != null && slotC.IdProvider.HasValue && slotC.IdProvider > 0)
                    proveedores.Add(new { id = slotC.IdProvider.Value, nombre = slotC.Solicit ?? $"Proveedor {slotC.IdProvider}" });

                // Obtener items del pedimento (solo los solicitados)
                var items = await _context.Detailsreqoc
                    .Where(d => d.IdMovement == pedimentoId &&
                                d.Active == true &&
                                d.Pedimento == true)
                    .AsNoTracking()
                    .ToListAsync();

                // Precargar todos los items de los 3 slots para evitar N+1 queries
                var itemsSlotA = slotA != null ? await _context.Detailsreqoc
                    .Where(d => d.IdMovement == slotA.Id && d.Active == true)
                    .AsNoTracking().ToListAsync() : new List<Detailsreqoc>();
                var itemsSlotB = slotB != null ? await _context.Detailsreqoc
                    .Where(d => d.IdMovement == slotB.Id && d.Active == true)
                    .AsNoTracking().ToListAsync() : new List<Detailsreqoc>();
                var itemsSlotC = slotC != null ? await _context.Detailsreqoc
                    .Where(d => d.IdMovement == slotC.Id && d.Active == true)
                    .AsNoTracking().ToListAsync() : new List<Detailsreqoc>();

                // Construir estructura de artículos con precios por proveedor
                var articulos = new List<object>();

                foreach (var item in items)
                {
                    var precios = new Dictionary<int, decimal>();
                    var comprasMinimas = new Dictionary<int, int>();
                    var tiemposEntrega = new Dictionary<int, string>();
                    var cantidades = new Dictionary<int, decimal>();
                    var slotItemIds = new Dictionary<int, int>();
                    var tiposOc = new Dictionary<int, string>();
                    var itemName = (item.NameArticle ?? item.Observation ?? "").Trim().ToLower();

                    Detailsreqoc? FindMatch(List<Detailsreqoc> slotItems) =>
                        // 1. Match exacto por IdSupplie (si no es 0)
                        (item.IdSupplie > 0 ? slotItems.FirstOrDefault(d => d.IdSupplie == item.IdSupplie) : null)
                        // 2. Fallback: match por nombre del artículo
                        ?? (!string.IsNullOrEmpty(itemName)
                            ? slotItems.FirstOrDefault(d =>
                                (d.NameArticle ?? d.Observation ?? "").Trim().ToLower() == itemName)
                            : null);

                    // Obtener precios, compra mínima, tiempo de entrega, cantidadConceptualizada y tipoOc de cada proveedor
                    if (slotA != null && slotA.IdProvider.HasValue && slotA.IdProvider > 0)
                    {
                        var precioA = FindMatch(itemsSlotA);
                        precios[slotA.IdProvider.Value] = precioA?.Price ?? item.Price;
                        comprasMinimas[slotA.IdProvider.Value] = (int)(precioA?.CompraMinima ?? item.CompraMinima ?? 1);
                        tiemposEntrega[slotA.IdProvider.Value] = precioA?.TiempoEntrega ?? item.TiempoEntrega ?? "";
                        cantidades[slotA.IdProvider.Value] = precioA?.CantidadConceptualizada ?? 0;
                        tiposOc[slotA.IdProvider.Value] = precioA?.TypeOc ?? "";
                        if (precioA != null) slotItemIds[slotA.IdProvider.Value] = precioA.Id;
                    }

                    if (slotB != null && slotB.IdProvider.HasValue && slotB.IdProvider > 0)
                    {
                        var precioB = FindMatch(itemsSlotB);
                        precios[slotB.IdProvider.Value] = precioB?.Price ?? item.Price;
                        comprasMinimas[slotB.IdProvider.Value] = (int)(precioB?.CompraMinima ?? item.CompraMinima ?? 1);
                        tiemposEntrega[slotB.IdProvider.Value] = precioB?.TiempoEntrega ?? item.TiempoEntrega ?? "";
                        cantidades[slotB.IdProvider.Value] = precioB?.CantidadConceptualizada ?? 0;
                        tiposOc[slotB.IdProvider.Value] = precioB?.TypeOc ?? "";
                        if (precioB != null) slotItemIds[slotB.IdProvider.Value] = precioB.Id;
                    }

                    if (slotC != null && slotC.IdProvider.HasValue && slotC.IdProvider > 0)
                    {
                        var precioC = FindMatch(itemsSlotC);
                        precios[slotC.IdProvider.Value] = precioC?.Price ?? item.Price;
                        comprasMinimas[slotC.IdProvider.Value] = (int)(precioC?.CompraMinima ?? item.CompraMinima ?? 1);
                        tiemposEntrega[slotC.IdProvider.Value] = precioC?.TiempoEntrega ?? item.TiempoEntrega ?? "";
                        cantidades[slotC.IdProvider.Value] = precioC?.CantidadConceptualizada ?? 0;
                        tiposOc[slotC.IdProvider.Value] = precioC?.TypeOc ?? "";
                        if (precioC != null) slotItemIds[slotC.IdProvider.Value] = precioC.Id;
                    }

                    articulos.Add(new
                    {
                        id = item.Id,
                        idSupplie = item.IdSupplie,
                        nombre = item.NameArticle ?? item.Observation ?? "",
                        numArticle = item.NumArticle ?? "",
                        cantidad = item.Quantity,
                        compraMinima = (int)(item.CompraMinima ?? 1),
                        recurrent = item.Recurrent ?? "Recurrente",
                        tiempoEntrega = item.TiempoEntrega ?? "",
                        comprasMinimas = comprasMinimas,
                        tiemposEntrega = tiemposEntrega,
                        precios = precios,
                        cantidades = cantidades,
                        tiposOc = tiposOc,
                        slotItemIds = slotItemIds
                    });
                }

                return new
                {
                    pedimentoId = pedimentoId,
                    proveedores = proveedores,
                    articulos = articulos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comparison data for pedimento {PedimentoId}", pedimentoId);
                throw;
            }
        }

        public async Task<List<object>> GetReqsByBranchMaterial(int idBranch, int idMaterial)
        {
            try
            {
                var result = await (
                    from o in _context.Ocandreqs
                    join d in _context.Detailsreqoc on o.Id equals d.IdMovement
                    where o.Active == true
                       && o.TypeReference == "branch"
                       && o.IdReference   == idBranch
                       && o.Type          == "REQUIS"
                       && d.IdSupplie     == idMaterial
                       && d.Active        == true
                    select new
                    {
                        o.Id,
                        o.Folio,
                        CantidadReq = d.Quantity,
                        NumCantidadOc = _context.Ocandreqs
                            .Count(oc => oc.Active == true
                                      && oc.Type   == "OC"
                                      && oc.IdReq  == o.Id
                                      && _context.Detailsreqoc
                                            .Any(dd => dd.IdMovement == oc.Id
                                                    && dd.IdSupplie  == idMaterial
                                                    && dd.Active     == true))
                    }
                ).Distinct().AsNoTracking().ToListAsync();

                return result.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reqs by branch {IdBranch} and material {IdMaterial}", idBranch, idMaterial);
                throw;
            }
        }
    }

    public interface IOcandreqService
    {
        Task<List<object>> GetOrders(string TypeReference, int idReference, string type);
        Task<List<object>> GetOcReq(int idRoot, string type);
        Task<object?> GetOrderById(int id);
        Task Save(Ocandreq ocandreq);
        Task<Ocandreq?> Update(int id, Ocandreq ocandreq);
        Task<Ocandreq?> UpdateAuthorization(int id, AuthorizationCallbackDto dto);
        Task<bool> Delete(int id);
        Task<Ocandreq?> SetLocked(int id, bool locked);
        Task<Ocandreq?> SetCountItem(int id, int countItem);
        Task<Ocandreq?> SetTotal(int id, decimal total);
        Task<List<ReqTypeOcFlagDto>> GetTypeOcFlags(List<int> reqIds);
        Task<object> GetComparisonData(int pedimentoId);
        Task<List<object>> GetReqsByBranchMaterial(int idBranch, int idMaterial);
    }

    public class ReqTypeOcFlagDto
    {
        public int ReqId       { get; set; }
        public bool HasNoAuth    { get; set; }
        public bool HasChangeSpec { get; set; }
    }
}