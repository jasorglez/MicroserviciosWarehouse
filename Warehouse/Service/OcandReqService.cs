
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

        public async Task<Ocandreq> Save(Ocandreq ocandreq)
        {
            try
            {
                ocandreq.DateModified = DateTime.Now;
                _context.Ocandreqs.Add(ocandreq);
                await _context.SaveChangesAsync();
                return ocandreq;
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
            _logger.LogInformation("🔒 SetLocked START - id={Id}, locked={Locked}", id, locked);

            var existingItem = await _context.Ocandreqs.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("🔒 SetLocked - Order with ID {Id} NOT FOUND", id);
                return null;
            }

            _logger.LogInformation("🔒 SetLocked - Found id={Id}, folio={Folio}, type={Type}, currentLocked={CurrentLocked}",
                existingItem.Id, existingItem.Folio, existingItem.Type, existingItem.Locked);

            try
            {
                existingItem.Locked = locked;
                _context.Entry(existingItem).Property(x => x.Locked).IsModified = true;

                var entityState = _context.Entry(existingItem).State;
                _logger.LogInformation("🔒 SetLocked - Before SaveChanges: newLocked={NewLocked}, entityState={State}",
                    existingItem.Locked, entityState);

                var rowsAffected = await _context.SaveChangesAsync();
                _logger.LogInformation("🔒 SetLocked DONE - id={Id}, rowsAffected={Rows}, finalLocked={FinalLocked}",
                    id, rowsAffected, existingItem.Locked);

                return existingItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔒 SetLocked ERROR - id={Id}", id);
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

        public async Task<Ocandreq?> SetTotalPedimento(int id, decimal totalPedimento)
        {
            var existingItem = await _context.Ocandreqs.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to update total_pedimento for non-existent Order with ID {Id}", id);
                return null;
            }

            try
            {
                existingItem.TotalPedimento = totalPedimento;
                await _context.SaveChangesAsync();
                return existingItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting total_pedimento for Order with ID {Id}", id);
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

                // ✅ Obtener TODOS los proveedores del pedimento (slots dinámicos N proveedores).
                // Folio nuevo: COTIZ-{branchPrefix}-P{ped}-PRO{idProvider}. Ya no usamos -A-/-B-/-C-.
                // Orden por Id ASC = orden de creación = slotIndex visual.
                var proveedorCotizs = await _context.Ocandreqs
                    .Where(o => o.TypeReference == "delison" &&
                                o.IdReference == pedimentoId &&
                                o.Type == "COTIZ" &&
                                o.Active == true)
                    .AsNoTracking()
                    .ToListAsync();

                var slotsOrdered = proveedorCotizs
                    .Where(c => c.IdProvider.HasValue && c.IdProvider > 0)
                    .OrderBy(c => c.Id)
                    .ToList();

                var proveedores = slotsOrdered
                    .Select(c => (object)new { id = c.IdProvider!.Value, nombre = c.Solicit ?? $"Proveedor {c.IdProvider}" })
                    .ToList();

                // Obtener items del pedimento (solo los solicitados)
                var items = await _context.Detailsreqoc
                    .Where(d => d.IdMovement == pedimentoId &&
                                d.Active == true &&
                                d.Pedimento == true)
                    .AsNoTracking()
                    .ToListAsync();

                // Precargar items de cada slot dinámico en un dict (slotId → items)
                var itemsBySlot = new Dictionary<int, List<Detailsreqoc>>();
                foreach (var slot in slotsOrdered)
                {
                    itemsBySlot[slot.Id] = await _context.Detailsreqoc
                        .Where(d => d.IdMovement == slot.Id && d.Active == true)
                        .AsNoTracking().ToListAsync();
                }

                // ✅ Precargar Insumo (Num Mat) actual del maestro materials para evitar snapshots desactualizados
                // Cuando se regenera el Num Mat en materiales-maestro (cambio de cat/fam/sub), el snapshot
                // en detailsreqoc.NumArticle queda obsoleto. Aquí devolvemos siempre el valor vigente del maestro.
                var idSuppliesUnique = items
                    .Where(i => i.IdSupplie > 0)
                    .Select(i => i.IdSupplie)
                    .Distinct()
                    .ToList();

                var materialsInsumoMap = idSuppliesUnique.Count > 0
                    ? await _context.Materials
                        .Where(m => idSuppliesUnique.Contains(m.Id))
                        .AsNoTracking()
                        .ToDictionaryAsync(m => m.Id, m => m.Insumo)
                    : new Dictionary<int, string?>();

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

                    // ✅ Iterar dinámicamente sobre TODOS los slots (N proveedores)
                    foreach (var slot in slotsOrdered)
                    {
                        var idProv = slot.IdProvider!.Value;
                        var slotItems = itemsBySlot[slot.Id];
                        var precio = FindMatch(slotItems);
                        precios[idProv] = precio?.Price ?? item.Price;
                        comprasMinimas[idProv] = (int)(precio?.CompraMinima ?? item.CompraMinima ?? 1);
                        tiemposEntrega[idProv] = precio?.TiempoEntrega ?? item.TiempoEntrega ?? "0";
                        cantidades[idProv] = precio?.CantidadConceptualizada ?? 0;
                        tiposOc[idProv] = precio?.TypeOc ?? "";
                        if (precio != null) slotItemIds[idProv] = precio.Id;
                    }

                    // Usar Insumo vigente del maestro si está disponible; fallback al snapshot
                    string liveNumArticle = item.NumArticle ?? "";
                    if (item.IdSupplie > 0 &&
                        materialsInsumoMap.TryGetValue(item.IdSupplie, out var insumoActual) &&
                        !string.IsNullOrWhiteSpace(insumoActual))
                    {
                        liveNumArticle = insumoActual;
                    }

                    articulos.Add(new
                    {
                        id = item.Id,
                        idSupplie = item.IdSupplie,
                        nombre = item.NameArticle ?? item.Observation ?? "",
                        numArticle = liveNumArticle,
                        cantidad = item.Quantity,
                        compraMinima = (int)(item.CompraMinima ?? 1),
                        recurrent = item.Recurrent ?? "Recurrente",
                        tiempoEntrega = item.TiempoEntrega ?? "0",
                        comprasMinimas = comprasMinimas,
                        tiemposEntrega = tiemposEntrega,
                        precios = precios,
                        cantidades = cantidades,
                        tiposOc = tiposOc,
                        slotItemIds = slotItemIds
                    });
                }

                // Verificar si la requisición vinculada está bloqueada (cubre el caso de Finalizar Req)
                // Los COTIZs (slots) tienen IdReq apuntando a la REQUIS padre
                var requisicionLocked = false;
                var reqId = slotsOrdered.FirstOrDefault()?.IdReq ?? 0;
                if (reqId > 0)
                {
                    var requisicion = await _context.Ocandreqs
                        .AsNoTracking()
                        .FirstOrDefaultAsync(o => o.Id == reqId);
                    requisicionLocked = requisicion?.Locked == true;
                }

                return new
                {
                    pedimentoId = pedimentoId,
                    locked = pedimento.Locked == true || requisicionLocked,
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

        public async Task<List<object>> GetReqsByBranchMaterial(int idBranch, int idMaterial, string? depts = null)
        {
            try
            {
                var departmentList = new List<int>();
                if (!string.IsNullOrEmpty(depts))
                {
                    departmentList = depts.Split(',')
                        .Select(d => int.TryParse(d.Trim(), out var result) ? result : 0)
                        .Where(d => d > 0)
                        .ToList();
                }
                else
                {
                    departmentList.Add(62);
                }

                // Query 1: obtener las requisiciones que aplican
                var reqs = await (
                    from o in _context.Ocandreqs
                    join d in _context.Detailsreqoc on o.Id equals d.IdMovement
                    where o.Active == true
                       && o.TypeReference == "branch"
                       && o.IdReference   == idBranch
                       && o.Type          == "REQUIS"
                       && departmentList.Contains(o.IdDepartament)
                       && d.IdSupplie     == idMaterial
                       && d.Active        == true
                    select new { o.Id, o.Folio, CantidadReq = d.Quantity }
                ).Distinct().AsNoTracking().ToListAsync();

                if (!reqs.Any())
                    return new List<object>();

                var reqIds = reqs.Select(r => r.Id).ToList();

                // Query 2: contar OCs que tengan el material específico (JOIN con Detailsreqoc)
                var ocCounts = await (
                    from oc in _context.Ocandreqs
                    join details in _context.Detailsreqoc on oc.Id equals details.IdMovement
                    where oc.Active == true
                       && oc.Type == "OC"
                       && oc.IdReq.HasValue
                       && reqIds.Contains(oc.IdReq!.Value)
                       && details.IdSupplie == idMaterial
                       && details.Active == true
                    group oc by oc.IdReq into g
                    select new { IdReq = g.Key, Count = g.Count() }
                )
                .AsNoTracking()
                .ToListAsync();

                var ocCountMap = ocCounts.ToDictionary(x => x.IdReq, x => x.Count);

                // Filtrar solo requisiciones que tengan OCs (NumCantidadOc > 0)
                var result = reqs
                    .Where(r => ocCountMap.ContainsKey(r.Id))
                    .Select(r => (object)new
                    {
                        r.Id,
                        r.Folio,
                        r.CantidadReq,
                        NumCantidadOc = ocCountMap[r.Id]
                    }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reqs by branch {IdBranch} and material {IdMaterial} depts {Depts}", idBranch, idMaterial, depts);
                throw;
            }
        }

        public async Task<List<object>> GetOcsByReqMaterial(int idReq, int idMaterial, string? depts = null)
        {
            try
            {
                var departmentList = new List<int>();
                if (!string.IsNullOrEmpty(depts))
                {
                    departmentList = depts.Split(',')
                        .Select(d => int.TryParse(d.Trim(), out var result) ? result : 0)
                        .Where(d => d > 0)
                        .ToList();
                }
                else
                {
                    departmentList.Add(62);
                }

                var result = await (
                    from oc in _context.Ocandreqs
                    join d in _context.Detailsreqoc on oc.Id equals d.IdMovement
                    where oc.Active == true
                       && oc.Type      == "OC"
                       && oc.IdReq     == idReq
                       && d.IdSupplie  == idMaterial
                       && d.Active     == true
                    select new
                    {
                        oc.Id,
                        oc.Folio,
                        oc.Close,
                        Proveedor    = d.NameProvider ?? d.ProvInt ?? "",
                        Cantidad     = d.Quantity,
                        Price       = d.Price,
                        CondEspecial = oc.Conditions ?? "",
                        Resta        = 0
                    }
                ).AsNoTracking().ToListAsync();

                return result.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OCs for req {IdReq} material {IdMaterial} depts {Depts}", idReq, idMaterial, depts);
                throw;
            }
        }

        public async Task<List<object>> GetOcsByRequisition(int? idRequisition)
        {
            try
            {
                if (!idRequisition.HasValue || idRequisition <= 0)
                    return new List<object>();

                var result = await _context.Ocandreqs
                    .Where(o =>
                        o.Active == true &&
                        o.Type == "OC" &&
                        o.IdReq == idRequisition)
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
                _logger.LogError(ex, "Error getting OCs for requisition {IdRequisition}", idRequisition);
                throw;
            }
        }

        public async Task<List<object>> GetOcsDetailsForRequisition(int? idRequisition)
        {
            try
            {
                if (!idRequisition.HasValue || idRequisition <= 0)
                    return new List<object>();

                var result = await (
                    from oc in _context.Ocandreqs
                    join d in _context.Detailsreqoc on oc.Id equals d.IdMovement
                    where oc.Active == true
                       && oc.Type == "OC"
                       && oc.IdReq == idRequisition
                       && d.Active == true
                    select new
                    {
                        oc.Id,
                        oc.Folio,
                        Proveedor = d.NameProvider ?? d.ProvInt ?? "",
                        Cantidad = d.Quantity,
                        CondEspecial = oc.Conditions ?? "",
                        Resta = 0
                    }
                ).OrderByDescending(x => x.Id)
                 .AsNoTracking()
                 .ToListAsync();

                return result.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OC details for requisition {IdRequisition}", idRequisition);
                throw;
            }
        }

        public async Task<List<object>> GetOcsByBranch(int idBranch)
        {
            try
            {
                if (idBranch <= 0) return new List<object>();

                // 1. Obtener requisiciones que tienen al menos un OC
                var groupedOcs = await (
                    from oc in _context.Ocandreqs
                    join req in _context.Ocandreqs on oc.IdReq equals req.Id
                    where oc.Active == true
                       && oc.Type == "OC"
                       && req.Active == true
                       && req.Type == "REQUIS"
                       && req.TypeReference == "branch"
                       && req.IdReference == idBranch
                    group oc by new
                    {
                        req.Id,
                        req.Folio,
                        req.IdReference,
                        oc.IdDepartament
                    } into grp
                    orderby grp.Max(x => x.DateModified) descending
                    select new
                    {
                        ReqId    = grp.Key.Id,
                        ReqFolio = grp.Key.Folio,
                        IdReference  = grp.Key.IdReference,
                        IdDepartament = grp.Key.IdDepartament,
                        DateModified = grp.Max(x => x.DateModified)
                    }
                ).AsNoTracking().ToListAsync();

                // 2. Contar pedimentos reales con OCs por requisición (misma lógica que GetPedimentosByRequisicion)
                var reqIds = groupedOcs.Select(x => x.ReqId).Distinct().ToList();

                var pedimentoCounts = await (
                    from cotiz in _context.Ocandreqs
                    where cotiz.Active == true
                       && cotiz.Type == "COTIZ"
                       && cotiz.Pedimento > 0
                       && cotiz.IdReq != null && reqIds.Contains(cotiz.IdReq.Value)
                       && _context.Ocandreqs.Any(oc =>
                              oc.Active == true
                           && oc.Type == "OC"
                           && oc.IdReq == cotiz.IdReq
                           && _context.Detailsreqoc.Any(d => d.IdMovement == oc.Id && d.Active == true)
                           && _context.Ocandreqs.Any(dc =>
                                  dc.Active == true
                               && dc.Type == "COTIZ"
                               && dc.TypeReference == "delison"
                               && dc.IdReference == cotiz.Id
                               && dc.IdProvider == oc.IdProvider))
                    group cotiz by cotiz.IdReq into grp
                    select new { ReqId = grp.Key, Count = grp.Count() }
                ).AsNoTracking().ToListAsync();

                var countMap = pedimentoCounts.ToDictionary(x => x.ReqId, x => x.Count);

                // 3. Obtener nombres de departamentos desde security.dbo.Roles
                var deptIds = groupedOcs.Select(x => x.IdDepartament).Distinct().Where(id => id > 0).ToList();
                var deptMap = new Dictionary<int, string>();

                if (deptIds.Any())
                {
                    var connection = _context.Database.GetDbConnection();
                    if (connection.State != System.Data.ConnectionState.Open)
                        await connection.OpenAsync();

                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = $"SELECT id, description FROM security.dbo.Roles WHERE id IN ({string.Join(",", deptIds)})";

                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                        deptMap[reader.GetInt32(0)] = reader.GetString(1);
                }

                // 4. Mapear en memoria con departamentos y contador correcto
                return groupedOcs.Select(grp => (object)new
                {
                    ReqId         = grp.ReqId,
                    ReqFolio      = grp.ReqFolio,
                    IdReference   = grp.IdReference,
                    IdDepartament = grp.IdDepartament,
                    DepartmentName   = deptMap.TryGetValue(grp.IdDepartament, out var deptName) ? deptName : "Sin Departamento",
                    CountPedimentos  = countMap.TryGetValue(grp.ReqId, out var cnt) ? cnt : 0,
                    DateModified  = grp.DateModified
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OCs for branch {IdBranch}", idBranch);
                throw;
            }
        }

        public async Task<List<object>> GetPedimentosByRequisicion(int idRequisicion)
        {
            try
            {
                if (idRequisicion <= 0) return new List<object>();

                // Buscar las COTIZs que son pedimentos (pedimento > 0) de la requisición
                // Solo mostrar las que tengan al menos un OC asignado, buscando via la cadena:
                // COTIZ pedimento → COTIZ delison (type_reference='delison', id_reference=cotiz.Id) → OC (mismo proveedor)
                var pedimentos = await (
                    from cotiz in _context.Ocandreqs
                    where cotiz.Active == true
                       && cotiz.Type == "COTIZ"
                       && cotiz.IdReq == idRequisicion
                       && cotiz.Pedimento > 0
                       && _context.Ocandreqs.Any(oc =>
                              oc.Active == true
                           && oc.Type == "OC"
                           && oc.IdReq == idRequisicion
                           && _context.Detailsreqoc.Any(d => d.IdMovement == oc.Id && d.Active == true)
                           && _context.Ocandreqs.Any(dc =>
                                  dc.Active == true
                               && dc.Type == "COTIZ"
                               && dc.TypeReference == "delison"
                               && dc.IdReference == cotiz.Id
                               && dc.IdProvider == oc.IdProvider))
                    orderby cotiz.Pedimento ascending
                    select new
                    {
                        cotiz.Id,
                        cotiz.Folio,
                        cotiz.IdReq,
                        cotiz.Pedimento,
                        cotiz.TotalPedimento,
                        cotiz.DateCreate,
                        cotiz.DateModified,
                        cotiz.Active
                    }
                ).AsNoTracking().ToListAsync();

                return pedimentos.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pedimentos for requisicion {IdRequisicion}", idRequisicion);
                throw;
            }
        }

        public async Task<bool> ShouldLockRequisicion(int idReq)
        {
            if (idReq <= 0) return false;

            // Pedimentos de la requisición
            var pedimentos = await _context.Ocandreqs
                .AsNoTracking()
                .Where(c => c.Active == true && c.Type == "COTIZ" && c.Pedimento > 0 && c.IdReq == idReq)
                .Select(c => c.Id)
                .ToListAsync();

            if (pedimentos.Count == 0) return false;

            // Pedimentos que ya tienen OC generada (lógica existente)
            var pedimentosConOc = await (
                from cotiz in _context.Ocandreqs
                where cotiz.Active == true
                   && cotiz.Type == "COTIZ"
                   && cotiz.IdReq == idReq
                   && cotiz.Pedimento > 0
                   && _context.Ocandreqs.Any(oc =>
                          oc.Active == true
                       && oc.Type == "OC"
                       && oc.IdReq == idReq
                       && _context.Detailsreqoc.Any(d => d.IdMovement == oc.Id && d.Active == true)
                       && _context.Ocandreqs.Any(dc =>
                              dc.Active == true
                           && dc.Type == "COTIZ"
                           && dc.TypeReference == "delison"
                           && dc.IdReference == cotiz.Id
                           && dc.IdProvider == oc.IdProvider))
                select cotiz.Id
            ).ToListAsync();

            // Pedimentos pendientes: los que NO tienen OC generada
            var pedimentosPendientes = pedimentos.Except(pedimentosConOc).ToList();

            // Si todos los pedimentos ya tienen OC, bloquear
            if (pedimentosPendientes.Count == 0) return true;

            // Verificar si los pedimentos pendientes están "Finalizados" (todos sus items con tipoOc negativo)
            var NOT_AUTHORIZED = new[] { "COMPRA NO AUTORIZADA", "CAMBIO DE ESPECIFICACIONES", "ARTICULO NO AUTORIZADO" };

            foreach (var pedimentoId in pedimentosPendientes)
            {
                // Slots COTIZ delison del pedimento (A/B/C)
                var slotIds = await _context.Ocandreqs
                    .AsNoTracking()
                    .Where(s => s.Active == true && s.Type == "COTIZ" && s.TypeReference == "delison" && s.IdReference == pedimentoId)
                    .Select(s => s.Id)
                    .ToListAsync();

                if (slotIds.Count == 0) return false;

                // Items de esos slots
                var itemTypeOcs = await _context.Detailsreqoc
                    .AsNoTracking()
                    .Where(d => slotIds.Contains(d.IdMovement) && d.Active == true)
                    .Select(d => d.TypeOc)
                    .ToListAsync();

                if (itemTypeOcs.Count == 0) return false;

                var allNegative = itemTypeOcs.All(t => !string.IsNullOrEmpty(t) && NOT_AUTHORIZED.Contains(t));
                if (!allNegative) return false;
            }

            return true;
        }

        public async Task<List<object>> GetOcsByPedimento(int idPedimento)
        {
            try
            {
                if (idPedimento <= 0) return new List<object>();

                // Obtener el pedimento para saber su requisición padre (idReq)
                var pedimento = await _context.Ocandreqs
                    .AsNoTracking()
                    .Where(p => p.Id == idPedimento && p.Active == true)
                    .FirstOrDefaultAsync();

                if (pedimento == null) return new List<object>();

                // Obtener los proveedores asociados al pedimento via sus sub-cotizaciones
                var providerIds = await _context.Ocandreqs
                    .AsNoTracking()
                    .Where(c => c.Type == "COTIZ"
                             && c.TypeReference == "delison"
                             && c.IdReference == idPedimento
                             && c.Active == true)
                    .Select(c => c.IdProvider)
                    .Where(id => id > 0)
                    .ToListAsync();

                if (!providerIds.Any()) return new List<object>();

                // OCs de la requisición padre que tengan alguno de esos proveedores
                var result = await _context.Ocandreqs
                    .Where(o => o.Type == "OC"
                             && o.IdReq == pedimento.IdReq
                             && providerIds.Contains(o.IdProvider)
                             && o.Active == true)
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
                _logger.LogError(ex, "Error getting OCs for pedimento {IdPedimento}", idPedimento);
                throw;
            }
        }
    }

    public interface IOcandreqService
    {
        Task<List<object>> GetOrders(string TypeReference, int idReference, string type);
        Task<List<object>> GetOcReq(int idRoot, string type);
        Task<object?> GetOrderById(int id);
        Task<Ocandreq> Save(Ocandreq ocandreq);
        Task<Ocandreq?> Update(int id, Ocandreq ocandreq);
        Task<Ocandreq?> UpdateAuthorization(int id, AuthorizationCallbackDto dto);
        Task<bool> Delete(int id);
        Task<Ocandreq?> SetLocked(int id, bool locked);
        Task<Ocandreq?> SetCountItem(int id, int countItem);
        Task<Ocandreq?> SetTotal(int id, decimal total);
        Task<Ocandreq?> SetTotalPedimento(int id, decimal totalPedimento);
        Task<List<ReqTypeOcFlagDto>> GetTypeOcFlags(List<int> reqIds);
        Task<object> GetComparisonData(int pedimentoId);
        Task<List<object>> GetReqsByBranchMaterial(int idBranch, int idMaterial, string? depts = null);
        Task<List<object>> GetOcsByReqMaterial(int idReq, int idMaterial, string? depts = null);
        Task<List<object>> GetOcsByRequisition(int? idRequisition);
        Task<List<object>> GetOcsDetailsForRequisition(int? idRequisition);
        Task<List<object>> GetOcsByBranch(int idBranch);
        Task<List<object>> GetPedimentosByRequisicion(int idRequisicion);
        Task<bool> ShouldLockRequisicion(int idReq);
        Task<List<object>> GetOcsByPedimento(int idPedimento);
    }

    public class ReqTypeOcFlagDto
    {
        public int ReqId       { get; set; }
        public bool HasNoAuth    { get; set; }
        public bool HasChangeSpec { get; set; }
    }
}
