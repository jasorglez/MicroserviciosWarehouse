
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
                        o.NumCotizacion,
                        o.CondicionesPago,
                        o.VigenciaCotizacion,
                        o.IdCondicionPago,
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
                        o.NumCotizacion,
                        o.CondicionesPago,
                        o.VigenciaCotizacion,
                        o.IdCondicionPago,
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
                        o.AuthorizedAt,
                        o.NumCotizacion,
                        o.CondicionesPago,
                        o.VigenciaCotizacion,
                        o.IdCondicionPago
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
                // ✅ DateModified SIEMPRE con hora del servidor (autoritativa).
                // Se ignora cualquier valor enviado por el cliente para evitar
                // desfases entre el reloj del navegador y el servidor, que provocaban
                // ordenamiento inconsistente en el grid de requisiciones (fila 3/5 en vez de arriba).
                existingItem.DateModified = DateTime.Now;
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
                    var comprasMinimas = new Dictionary<int, decimal>();   // decimal: compra mínima admite 1 decimal
                    var tiemposEntrega = new Dictionary<int, string>();
                    var cantidades = new Dictionary<int, decimal>();
                    var slotItemIds = new Dictionary<int, int>();
                    var tiposOc = new Dictionary<int, string>();
                    var masIvasPorProv = new Dictionary<int, bool>();
                    var monedasPorProv = new Dictionary<int, int?>();   // Fase 2: moneda del precio por proveedor (NULL = MXN)
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
                        comprasMinimas[idProv] = precio?.CompraMinima ?? item.CompraMinima ?? 1;
                        tiemposEntrega[idProv] = precio?.TiempoEntrega ?? item.TiempoEntrega ?? "0";
                        cantidades[idProv] = precio?.CantidadConceptualizada ?? 0;
                        tiposOc[idProv] = precio?.TypeOc ?? "";
                        masIvasPorProv[idProv] = precio?.MasIva ?? false;
                        monedasPorProv[idProv] = precio?.IdCurrency;   // Fase 2: hereda la moneda del slot (NULL = MXN)
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
                        compraMinima = item.CompraMinima ?? 1,
                        idProveedorSugerido = item.IdProveedorSugerido,  // proveedor sugerido por el panel (REQUIS)
                        recurrent = item.Recurrent ?? "Recurrente",
                        tiempoEntrega = item.TiempoEntrega ?? "0",
                        comprasMinimas = comprasMinimas,
                        tiemposEntrega = tiemposEntrega,
                        precios = precios,
                        cantidades = cantidades,
                        tiposOc = tiposOc,
                        masIvas = masIvasPorProv,
                        monedas = monedasPorProv,
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

                // Query 2: contar OCs por requisición. Total = tiene OCs (para que la requisición siga
                // apareciendo); Liberadas = solo las liberadas al almacén (lo que se muestra en # OC).
                var ocCounts = await (
                    from oc in _context.Ocandreqs
                    join details in _context.Detailsreqoc on oc.Id equals details.IdMovement
                    where oc.Active == true
                       && oc.Type == "OC"
                       && oc.IdReq.HasValue
                       && reqIds.Contains(oc.IdReq!.Value)
                       && details.IdSupplie == idMaterial
                       && details.Active == true
                    group new { oc, details } by oc.IdReq into g
                    select new
                    {
                        IdReq = g.Key,
                        Total = g.Count(),
                        Liberadas = g.Sum(x => x.details.LiberarAlmacen ? 1 : 0)
                    }
                )
                .AsNoTracking()
                .ToListAsync();

                var ocAnyMap      = ocCounts.ToDictionary(x => x.IdReq, x => x.Total);
                var ocLiberadasMap = ocCounts.ToDictionary(x => x.IdReq, x => x.Liberadas);

                // La requisición SIGUE apareciendo si tiene OCs (liberadas o no); # OC = solo liberadas.
                var result = reqs
                    .Where(r => ocAnyMap.ContainsKey(r.Id))
                    .Select(r => (object)new
                    {
                        r.Id,
                        r.Folio,
                        r.CantidadReq,
                        NumCantidadOc = ocLiberadasMap.TryGetValue(r.Id, out var lib) ? lib : 0
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

                // Folio y sucursal de la REQUIS padre (para componer el Folio CR de las Compras Rápidas).
                var reqParent = await _context.Ocandreqs
                    .Where(r => r.Id == idReq)
                    .Select(r => new { r.Folio, r.IdReference })
                    .FirstOrDefaultAsync();
                var reqFolioNoDash = (reqParent?.Folio ?? "").Replace("-", "");
                // Iniciales del proveedor para el folio CR (consecutive_oc_proveedor de la sucursal, default 3).
                var providerInitials = await _context.PrefixSetups
                    .Where(p => p.IdProjectOrBranch == (reqParent != null ? reqParent.IdReference : 0)
                                && p.Type == "branch" && p.Active)
                    .Select(p => p.ConsecutiveOcProveedor)
                    .FirstOrDefaultAsync() ?? 3;
                if (providerInitials <= 0) providerInitials = 3;

                var result = await (
                    from oc in _context.Ocandreqs
                    join d in _context.Detailsreqoc on oc.Id equals d.IdMovement
                    where oc.Active == true
                       && (oc.Type == "OC" || oc.Type == "CR")   // incluye documentos de Compra Rápida
                       && oc.IdReq     == idReq
                       && d.IdSupplie  == idMaterial
                       && d.Active     == true
                       && d.LiberarAlmacen == true                // gate: solo ítems liberados al almacén
                    select new
                    {
                        oc.Id,
                        oc.Folio,
                        oc.IdDepartament,
                        // Close POR ARTÍCULO (línea), no por OC global: si la línea tiene entregas,
                        // está cerrada cuando todas sus entregas están cerradas; si no tiene entregas,
                        // cae al close de la OC (CR / legacy). Así cerrar un artículo de 1 entrega no
                        // bloquea a los demás artículos de la misma OC.
                        Close = _context.EntregasOc.Any(eg => eg.IdDetailsreqoc == d.Id && eg.Active)
                                ? !_context.EntregasOc.Any(eg => eg.IdDetailsreqoc == d.Id && eg.Active && eg.Close == false)
                                : oc.Close,
                        oc.Type,
                        TipoOc       = d.TypeOc,   // 'COMPRA INMEDIATA', 'COMPRA AUTORIZADA EN OTRA FECHA', etc. (tipo OC del detalle)
                        IdDetail     = d.Id,
                        Proveedor    = d.NameProvider ?? d.ProvInt ?? "",
                        IdProvider   = oc.IdProvider ?? 0,
                        // CR pagada (entrada liberada): habilita el sufijo de proveedor en el folio CR.
                        Paid         = _context.EntradasMolienda.Any(e => e.IdOc == oc.Id && e.Active && e.Liberacion),
                        Cantidad     = d.Quantity,
                        Price       = d.Price,
                        MasIva       = d.MasIva,
                        // Moneda del ítem (ISO) para mostrar/convertir en almacén molienda (Fase 4).
                        Moneda      = _context.Catalogs.Where(c => c.Id == d.IdCurrency).Select(c => c.ValueAddition).FirstOrDefault(),
                        CondEspecial = oc.Conditions ?? "",
                        CantidadMinimaRequerida = d.CaducidadMinimaRequerida,
                        FechaXEntrega = d.DatePostpone,
                        Resta        = 0,
                        DiasCondicionCompra = d.DiasCondicionCompra ?? 1,
                        EntregasCount = _context.EntregasOc.Count(e => e.IdDetailsreqoc == d.Id && e.Active)
                    }
                ).AsNoTracking().ToListAsync();

                // Prefijo de departamento (security.dbo.Roles.prefijo) para componer el Folio CR
                // de las Compras Rápidas: CR-{folioReq sin guion}-{prefijoDepto} (ej. CR-BOD9-GO).
                var crDeptIds = result.Where(x => x.Type == "CR")
                    .Select(x => x.IdDepartament).Distinct().Where(id => id > 0).ToList();
                var deptPrefixMap = new Dictionary<int, string>();
                if (crDeptIds.Any())
                {
                    var connection = _context.Database.GetDbConnection();
                    if (connection.State != System.Data.ConnectionState.Open)
                        await connection.OpenAsync();
                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = $"SELECT id, prefijo FROM security.dbo.Roles WHERE id IN ({string.Join(",", crDeptIds)})";
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                        deptPrefixMap[reader.GetInt32(0)] = reader.IsDBNull(1) ? "" : reader.GetString(1);
                }

                return result.Select(x =>
                {
                    var deptPrefix = deptPrefixMap.TryGetValue(x.IdDepartament, out var pf) ? pf : "";
                    var folioFinal = x.Type == "CR"
                        ? $"CR-{reqFolioNoDash}"
                          + (string.IsNullOrWhiteSpace(deptPrefix) ? "" : $"-{deptPrefix}")
                          + (x.Paid ? BuildProviderSuffix(x.Proveedor, x.IdProvider, providerInitials) : "")
                        : x.Folio;
                    return (object)new
                    {
                        x.Id,
                        Folio = folioFinal,
                        x.Close,
                        x.Type,
                        x.TipoOc,
                        x.IdDetail,
                        x.Proveedor,
                        x.Cantidad,
                        x.Price,
                        x.MasIva,
                        x.Moneda,
                        x.CondEspecial,
                        x.CantidadMinimaRequerida,
                        x.FechaXEntrega,
                        x.Resta,
                        x.DiasCondicionCompra,
                        x.EntregasCount
                    };
                }).ToList();
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
                        o.NumCotizacion,
                        o.CondicionesPago,
                        o.VigenciaCotizacion,
                        o.IdCondicionPago,
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

        // Sufijo de proveedor para el folio CR: -{abreviatura}{id}, donde abreviatura = primeras N
        // letras del nombre del proveedor en mayúsculas (misma regla que el folio de OC). Vacío si la
        // CR aún no tiene proveedor (no pagada). N = consecutive_oc_proveedor de la sucursal (default 3).
        private static string BuildProviderSuffix(string? providerName, int providerId, int initials)
        {
            if (providerId <= 0 || string.IsNullOrWhiteSpace(providerName)) return "";
            if (initials <= 0) initials = 3;
            var code = providerName.Trim().ToUpperInvariant();
            if (code.Length > initials) code = code.Substring(0, initials);
            return $"-{code}{providerId}";
        }

        public async Task<List<object>> GetCompraRapidaItems(int idBranch, int idMaterial = 0)
        {
            try
            {
                if (idBranch <= 0) return new List<object>();

                // Items con compra rápida (comprarapida=true) de las REQUIS de la sucursal.
                // Si idMaterial > 0, se filtra por material (usado por almacén molienda, que es por material).
                var items = await (
                    from d in _context.Detailsreqoc
                    join req in _context.Ocandreqs on d.IdMovement equals req.Id
                    where d.Active == true
                       && d.CompraRapida == true
                       && req.Active == true
                       && req.Type == "REQUIS"
                       && req.TypeReference == "branch"
                       && req.IdReference == idBranch
                       && (idMaterial == 0 || d.IdSupplie == idMaterial)
                    orderby req.DateModified descending, d.Id
                    select new
                    {
                        Id            = d.Id,
                        ReqId         = req.Id,
                        ReqFolio      = req.Folio,
                        IdReference   = req.IdReference,
                        IdDepartament = req.IdDepartament,
                        IdSupplie     = d.IdSupplie,
                        RequestDate   = req.DateCreate,
                        SolicitedBy   = req.Solicit,
                        Recurrent     = d.Recurrent,
                        Article       = d.NameArticle,
                        NumArticle    = d.NumArticle,
                        Quantity      = d.Quantity,
                        Comment       = d.Comment,
                        Price         = d.Price,
                        Total         = d.Total,
                        CaducidadMinimaRequerida = d.CaducidadMinimaRequerida,
                        // Id del documento CR generado para este item (para compartir documentos PDF).
                        CrId = _context.Ocandreqs
                            .Where(c => c.Type == "CR" && c.IdReference == d.Id && c.Active == true)
                            .Select(c => (int?)c.Id)
                            .FirstOrDefault()
                    }
                ).AsNoTracking().ToListAsync();

                if (!items.Any()) return new List<object>();

                // ✅ Live lookup del insumo (Num Mat) vigente del maestro de materiales.
                // detailsreqoc.numarticle es un snapshot que puede quedar obsoleto si el material
                // fue reclasificado después de crear la requisición. Mismo patrón que GetComparisonData.
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

                // Nombres y PREFIJOS de departamentos desde security.dbo.Roles (mismo origen que GetOcsByBranch).
                // El prefijo se usa para componer el Folio CR (CR-{sucursalSinGuion}-{prefijoDepto}).
                var deptIds = items.Select(x => x.IdDepartament).Distinct().Where(id => id > 0).ToList();
                var deptMap = new Dictionary<int, string>();
                var deptPrefixMap = new Dictionary<int, string>();
                if (deptIds.Any())
                {
                    var connection = _context.Database.GetDbConnection();
                    if (connection.State != System.Data.ConnectionState.Open)
                        await connection.OpenAsync();

                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = $"SELECT id, description, prefijo FROM security.dbo.Roles WHERE id IN ({string.Join(",", deptIds)})";

                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var did = reader.GetInt32(0);
                        deptMap[did] = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        deptPrefixMap[did] = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    }
                }

                // ── Datos capturados al PAGAR la Compra Rápida (Hoja de Gastos) ──
                // Proveedor y precio base viven en el detalle del documento CR; pago/nota/fecha/cantidad
                // viven en su entrada (entradas_molienda). Una CR tiene a lo más UNA entrada.
                var crIds = items.Where(x => x.CrId.HasValue).Select(x => x.CrId!.Value).Distinct().ToList();
                var crDetalleMap = new Dictionary<int, (string? Provider, int ProviderId, decimal Price, bool MasIva)>();
                var crEntradaMap = new Dictionary<int, (decimal Pago, string? Nota, DateOnly? Fecha, decimal Cantidad, bool Liberacion, decimal? MontoMxn, string? Moneda, decimal? TipoCambio)>();
                if (crIds.Any())
                {
                    var crDetalles = await _context.Detailsreqoc
                        .Where(dd => crIds.Contains(dd.IdMovement) && dd.Active == true)
                        .Select(dd => new { dd.IdMovement, dd.NameProvider, dd.IdProvider, dd.Price, dd.MasIva })
                        .ToListAsync();
                    foreach (var cd in crDetalles)
                        if (!crDetalleMap.ContainsKey(cd.IdMovement))
                            crDetalleMap[cd.IdMovement] = (cd.NameProvider, cd.IdProvider ?? 0, cd.Price, cd.MasIva ?? false);

                    var crEntradas = await _context.EntradasMolienda
                        .Where(e => crIds.Contains(e.IdOc) && e.Active)
                        .Select(e => new { e.IdOc, e.Pago, e.NotaFactura, e.FechaRecepcion, e.CantidadEntrada, e.Liberacion, e.MontoMxn, e.Moneda, e.TipoCambio })
                        .ToListAsync();
                    foreach (var ce in crEntradas)
                        if (!crEntradaMap.ContainsKey(ce.IdOc))
                            crEntradaMap[ce.IdOc] = (ce.Pago ?? 0m, ce.NotaFactura, ce.FechaRecepcion, ce.CantidadEntrada ?? 0m, ce.Liberacion, ce.MontoMxn, ce.Moneda, ce.TipoCambio);
                }

                // IVA% de la sucursal (setup almacén) para mostrar el precio unitario igual que en Gastos:
                // base si mas_iva = false; base + IVA (round2, Opción A) si mas_iva = true.
                var ivaPctCr = await _context.Setups
                    .Where(s => s.IdBranch == idBranch && s.Active)
                    .Select(s => s.Iva)
                    .FirstOrDefaultAsync() ?? 0m;
                var ivaFactorCr = 1m + ivaPctCr / 100m;

                // Iniciales del proveedor para el folio CR (consecutive_oc_proveedor de la sucursal, default 3).
                var providerInitials = await _context.PrefixSetups
                    .Where(p => p.IdProjectOrBranch == idBranch && p.Type == "branch" && p.Active)
                    .Select(p => p.ConsecutiveOcProveedor)
                    .FirstOrDefaultAsync() ?? 3;
                if (providerInitials <= 0) providerInitials = 3;

                return items.Select(x =>
                {
                    var crId = x.CrId ?? 0;
                    crDetalleMap.TryGetValue(crId, out var det);   // default: null/0/false si no hay CR
                    crEntradaMap.TryGetValue(crId, out var ent);   // default: 0/null si no hay entrada
                    var precioUnit = det.MasIva ? Math.Round(det.Price * ivaFactorCr, 2) : det.Price;
                    // Precio unitario en MXN: si moneda extranjera, aplica el TC guardado en entradas_molienda.
                    var monedaCr = string.IsNullOrWhiteSpace(ent.Moneda) ? "MXN" : ent.Moneda;
                    decimal precioUnitMxn = precioUnit;
                    if (monedaCr != "MXN" && ent.TipoCambio.HasValue && ent.TipoCambio.Value > 0)
                        precioUnitMxn = Math.Round(precioUnit * ent.TipoCambio.Value, 2);
                    // Folio CR: CR-{folioReq sin guiones}-{prefijo del departamento}[-{abrev}{idProveedor}].
                    // El sufijo de proveedor aparece una vez pagada la CR (ya tiene proveedor + id).
                    var deptPrefix = deptPrefixMap.TryGetValue(x.IdDepartament, out var dp) ? dp : "";
                    var reqFolioNoDash = (x.ReqFolio ?? "").Replace("-", "");
                    var folioCr = $"CR-{reqFolioNoDash}" + (string.IsNullOrWhiteSpace(deptPrefix) ? "" : $"-{deptPrefix}");
                    // Sufijo de proveedor solo cuando la CR ya está pagada (entrada liberada).
                    if (ent.Liberacion)
                        folioCr += BuildProviderSuffix(det.Provider, det.ProviderId, providerInitials);
                    return (object)new
                    {
                        x.Id,
                        x.ReqId,
                        x.ReqFolio,
                        FolioCr = folioCr,
                        x.IdReference,
                        x.IdDepartament,
                        x.IdSupplie,
                        x.RequestDate,
                        DepartmentName = deptMap.TryGetValue(x.IdDepartament, out var dn) ? dn : "Sin Departamento",
                        x.SolicitedBy,
                        x.Recurrent,
                        x.Article,
                        NumArticle = (x.IdSupplie > 0
                            && materialsInsumoMap.TryGetValue(x.IdSupplie, out var liveInsumo)
                            && !string.IsNullOrWhiteSpace(liveInsumo))
                            ? liveInsumo
                            : x.NumArticle,
                        x.Quantity,
                        x.Comment,
                        x.Price,
                        x.Total,
                        x.CaducidadMinimaRequerida,
                        x.CrId,
                        // ── Datos del pago de la Compra Rápida (para columna Proveedor + tooltip) ──
                        Proveedor = det.Provider,
                        PrecioUnitario = precioUnit,             // base o base+IVA en moneda original
                        PrecioUnitarioMxn = precioUnitMxn,       // precio convertido a MXN (celda)
                        PrecioUnitarioOriginal = precioUnit,     // precio en moneda original (tooltip si ≠ MXN)
                        TotalPagado = ent.Pago,                  // monto realmente pagado (con/sin IVA según el check)
                        // Total CR: costo total en MXN (monto_mxn = pago × TC). Solo cuando la CR ya
                        // está pagada (entrada liberada); antes va null para que la celda quede vacía.
                        TotalCr = ent.Liberacion ? (ent.MontoMxn ?? ent.Pago) : (decimal?)null,
                        Moneda = monedaCr,
                        NotaFactura = ent.Nota,
                        FechaEntradaAlmacen = ent.Fecha,
                        CantidadEntradaAlmacen = ent.Cantidad
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting compra rapida items for branch {IdBranch}", idBranch);
                throw;
            }
        }

        /// <summary>
        /// Sincroniza los documentos de Compra Rápida (ocandreq type='CR') de una requisición.
        /// - Crea un CR por cada item con comprarapida=true que aún no lo tenga (folio CR-{folioReq}-{n}),
        ///   copiando el item a detailsreqoc (id_movement = CR.id). Idempotente (link por IdReference=item.id).
        /// - Sincroniza el item copiado si el origen cambió (cantidad, precio, etc.).
        /// - Borra (duro) los CR cuyo item origen ya no es compra rápida y NO tienen entradas en almacén.
        /// </summary>
        public async Task<object> SyncCompraRapida(int idReq)
        {
            if (idReq <= 0) return new { created = 0, updated = 0, deleted = 0 };

            var req = await _context.Ocandreqs
                .FirstOrDefaultAsync(o => o.Id == idReq && o.Type == "REQUIS" && o.Active == true);
            if (req == null) return new { error = "REQUIS no encontrada" };

            var reqItems = await _context.Detailsreqoc
                .Where(d => d.IdMovement == idReq && d.Active == true)
                .ToListAsync();
            var crItems = reqItems.Where(d => d.CompraRapida == true).ToList();

            var existingCRs = await _context.Ocandreqs
                .Where(o => o.Type == "CR" && o.IdReq == idReq && o.Active == true)
                .ToListAsync();

            // Consecutivo n: max existente + 1 (parseado del folio CR-...-n).
            int nextN = 1;
            foreach (var c in existingCRs)
            {
                var parts = (c.Folio ?? string.Empty).Split('-');
                if (parts.Length > 0 && int.TryParse(parts[parts.Length - 1], out var nn) && nn >= nextN)
                    nextN = nn + 1;
            }

            int created = 0, updated = 0, deleted = 0;

            // 1) Crear / sincronizar por cada item con compra rápida
            foreach (var item in crItems)
            {
                var cr = existingCRs.FirstOrDefault(c => c.IdReference == item.Id);
                if (cr == null)
                {
                    var newCr = new Ocandreq
                    {
                        IdRoot        = req.IdRoot,
                        Folio         = $"CR-{req.Folio}-{nextN}",
                        TypeReference = "compra_rapida",
                        IdReq         = idReq,
                        IdReference   = item.Id,            // link al item origen (idempotencia)
                        DateCreate    = req.DateCreate,
                        IdProvider    = 0,
                        IdDepartament = req.IdDepartament,
                        TypeOc        = req.TypeOc ?? "INSUMOS",
                        Type          = "CR",
                        Solicit       = req.Solicit,
                        Close         = false,
                        Active        = true,
                        DateModified  = DateTime.Now,
                    };
                    _context.Ocandreqs.Add(newCr);
                    await _context.SaveChangesAsync();
                    nextN++;

                    var copy = new Detailsreqoc
                    {
                        IdMovement   = newCr.Id,
                        IdSupplie    = item.IdSupplie,
                        IdProvider   = 0,
                        NameProvider = null,                // Proveedor pendiente (hoja gastos)
                        Quantity     = item.Quantity,
                        Price        = 0,                   // Precio pendiente (hoja gastos) → Pago = 0
                        Dateuse      = item.Dateuse,
                        Type         = "CR",
                        Recurrent    = item.Recurrent,
                        NameArticle  = item.NameArticle,
                        NumArticle   = item.NumArticle,
                        Intorext     = item.Intorext,
                        ProvInt      = null,                // Proveedor pendiente (hoja gastos)
                        TypePriority = item.TypePriority,
                        Comment      = item.Comment,
                        Observation  = item.Observation,
                        CaducidadMinimaRequerida = item.CaducidadMinimaRequerida,
                        DiasCondicionCompra = 1,            // single-entrega
                        DatePostpone = req.DateCreate,      // Fecha x Entrega = Fecha solicitud de la REQUIS
                        Active       = true,
                        CompraRapida = false,               // el copiado NO es el origen
                    };
                    _context.Detailsreqoc.Add(copy);
                    await _context.SaveChangesAsync();
                    created++;
                }
                else
                {
                    // Sincronizar el item copiado con el origen
                    var copy = await _context.Detailsreqoc
                        .FirstOrDefaultAsync(d => d.IdMovement == cr.Id && d.Active == true);
                    if (copy != null)
                    {
                        // No se sincroniza Price/Proveedor: vienen de la hoja gastos (futuro), no de la requisición.
                        copy.IdSupplie   = item.IdSupplie;
                        copy.Quantity    = item.Quantity;
                        copy.NameArticle = item.NameArticle;
                        copy.NumArticle  = item.NumArticle;
                        copy.Comment     = item.Comment;
                        copy.CaducidadMinimaRequerida = item.CaducidadMinimaRequerida;
                        cr.DateModified  = DateTime.Now;
                        await _context.SaveChangesAsync();
                        updated++;
                    }
                }
            }

            // 2) Borrar (duro) CR cuyo item origen ya no es compra rápida y SIN entradas en almacén
            var crItemIds = crItems.Select(i => i.Id).ToHashSet();
            foreach (var cr in existingCRs)
            {
                if (crItemIds.Contains(cr.IdReference)) continue; // sigue siendo compra rápida
                var hasEntradas = await _context.EntradasMolienda.AnyAsync(e => e.IdOc == cr.Id && e.Active);
                if (hasEntradas) continue; // protegido (el front bloquea antes; esto es defensivo)

                var copies = await _context.Detailsreqoc.Where(d => d.IdMovement == cr.Id).ToListAsync();
                _context.Detailsreqoc.RemoveRange(copies);
                _context.Ocandreqs.Remove(cr);
                await _context.SaveChangesAsync();
                deleted++;
            }

            return new { created, updated, deleted };
        }

        /// <summary>Indica si el item de compra rápida (detailsreqoc.id origen) ya tiene entradas en almacén
        /// (a través de su documento CR). Usado para bloquear desmarcar/borrar en Requisiciones.</summary>
        public async Task<bool> CompraRapidaHasEntradas(int idItem)
        {
            if (idItem <= 0) return false;
            var crIds = await _context.Ocandreqs
                .Where(o => o.Type == "CR" && o.IdReference == idItem && o.Active == true)
                .Select(o => o.Id)
                .ToListAsync();
            if (!crIds.Any()) return false;
            return await _context.EntradasMolienda.AnyAsync(e => crIds.Contains(e.IdOc) && e.Active);
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
                        cotiz.DateCreate,
                        cotiz.DateModified,
                        cotiz.Active
                    }
                ).AsNoTracking().ToListAsync();

                // IVA% de la sucursal de la requisición (mismo origen que la cotización: setup de almacén por branch).
                // El precio guardado en detailsreqoc es BASE (Opción B), así que el IVA se aplica al sumar.
                var idBranchReq = await _context.Ocandreqs
                    .Where(o => o.Id == idRequisicion)
                    .Select(o => o.IdReference)
                    .FirstOrDefaultAsync();
                var ivaPercent = await _context.Setups
                    .Where(s => s.IdBranch == idBranchReq && s.Active)
                    .Select(s => s.Iva)
                    .FirstOrDefaultAsync() ?? 0m;
                var ivaFactor = 1m + ivaPercent / 100m;

                // TotalPedimento se calcula EN VIVO desde los items de los OCs que pertenecen
                // a cada pedimento (mismos providers via la cadena de COTIZs delison).
                // Esto evita drift entre snapshot y items editados después.
                var result = new List<object>();
                foreach (var ped in pedimentos)
                {
                    var lineas = await _context.Detailsreqoc
                        .Where(d => d.Active == true
                                 && _context.Ocandreqs.Any(oc =>
                                        oc.Id == d.IdMovement
                                     && oc.Active == true
                                     && oc.Type == "OC"
                                     && oc.IdReq == idRequisicion
                                     && _context.Ocandreqs.Any(dc =>
                                            dc.Active == true
                                         && dc.Type == "COTIZ"
                                         && dc.TypeReference == "delison"
                                         && dc.IdReference == ped.Id
                                         && dc.IdProvider == oc.IdProvider)))
                        .Select(d => new { d.Id, d.Quantity, d.Price, d.MasIva })
                        .ToListAsync();

                    // Total POR ENTREGA (igual que Nivel 4/5 y Total x OC): para cada ítem multi-entrega,
                    // Σ de sus entregas round2(precio × IVA de la entrega) × (cantidad de almacén si entró,
                    // o la planeada). Ítems sin entregas → precio × cantidad con el IVA del ítem.
                    var itemIdsPed = lineas.Select(l => l.Id).ToList();
                    var entregasPed = itemIdsPed.Count > 0
                        ? await _context.EntregasOc.AsNoTracking()
                            .Where(g => itemIdsPed.Contains(g.IdDetailsreqoc) && g.Active)
                            .Select(g => new { g.Id, g.IdDetailsreqoc, g.MasIva, g.CantidadRecibir })
                            .ToListAsync()
                        : new();
                    var entregaIdsPed = entregasPed.Select(g => g.Id).ToList();
                    var almacenByEntregaPed = entregaIdsPed.Count > 0
                        ? (await _context.EntradasMolienda.AsNoTracking()
                            .Where(e => e.IdEntrega != null && entregaIdsPed.Contains(e.IdEntrega.Value) && e.Active)
                            .GroupBy(e => e.IdEntrega!.Value)
                            .Select(g => new { Entrega = g.Key, Qty = g.Sum(x => x.CantidadEntrada ?? 0m) })
                            .ToListAsync())
                            .ToDictionary(x => x.Entrega, x => x.Qty)
                        : new Dictionary<int, decimal>();

                    decimal totalLive = 0m;
                    foreach (var l in lineas)
                    {
                        var itemEntregas = entregasPed.Where(g => g.IdDetailsreqoc == l.Id).ToList();
                        if (itemEntregas.Count > 0)
                        {
                            foreach (var g in itemEntregas)
                            {
                                var factor = g.MasIva ? ivaFactor : 1m;
                                var unit = Math.Round(l.Price * factor, 2);
                                var qa = almacenByEntregaPed.TryGetValue(g.Id, out var q) ? q : 0m;
                                var qty = qa > 0 ? qa : (g.CantidadRecibir ?? 0m);
                                totalLive += unit * qty;
                            }
                        }
                        else
                        {
                            var factor = (l.MasIva ?? false) ? ivaFactor : 1m;
                            totalLive += Math.Round(l.Price * factor, 2) * l.Quantity;
                        }
                    }

                    result.Add(new
                    {
                        ped.Id,
                        ped.Folio,
                        ped.IdReq,
                        ped.Pedimento,
                        TotalPedimento = totalLive,
                        ped.DateCreate,
                        ped.DateModified,
                        ped.Active
                    });
                }

                return result;
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

                // Cabecera de cotización por proveedor (num_cotizacion / condiciones_pago / vigencia)
                // se captura en el COTIZ hermano del OC (mismo IdReq + IdProvider). Lo precargamos
                // una sola vez para mezclarlo después y evitar subqueries por fila.
                var cotizData = await _context.Ocandreqs
                    .AsNoTracking()
                    .Where(c => c.Type == "COTIZ"
                             && c.IdReq == pedimento.IdReq
                             && providerIds.Contains(c.IdProvider)
                             && c.Active == true)
                    .Select(c => new { c.IdProvider, c.NumCotizacion, c.CondicionesPago, c.VigenciaCotizacion, c.IdCondicionPago })
                    .ToListAsync();
                var cotizByProvider = cotizData
                    .GroupBy(c => c.IdProvider)
                    .ToDictionary(g => g.Key, g => g.First());

                // Resolver la empresa (IdRoot) de forma robusta. Tanto el PEDIMENTO como las COTIZ
                // delison hijas pueden tener IdRoot=0 en datos viejos, lo que rompería el cálculo del
                // Anticipo OC (catálogo vacío). Cadena de fallbacks por confiabilidad:
                //   1) COTIZ delison hija con IdRoot poblado
                //   2) OC del pedimento con IdRoot poblado (la OC sí lo trae correcto)
                //   3) Empresa de la sucursal de la REQUIS padre (smp.dbo.Branchs → fuente 100% confiable)
                //   4) pedimento.IdRoot
                int idCompany =
                    await _context.Ocandreqs.AsNoTracking()
                        .Where(c => c.Type == "COTIZ" && c.TypeReference == "delison"
                                 && c.IdReference == idPedimento && c.Active == true
                                 && c.IdRoot != null && c.IdRoot != 0)
                        .Select(c => c.IdRoot).FirstOrDefaultAsync() ?? 0;

                if (idCompany == 0)
                {
                    idCompany = await _context.Ocandreqs.AsNoTracking()
                        .Where(o => o.Type == "OC" && o.IdReq == pedimento.IdReq
                                 && providerIds.Contains(o.IdProvider) && o.Active == true
                                 && o.IdRoot != null && o.IdRoot != 0)
                        .Select(o => o.IdRoot).FirstOrDefaultAsync() ?? 0;
                }

                if (idCompany == 0)
                {
                    // Sucursal de la REQUIS padre → empresa en smp.dbo.Branchs.
                    var idBranchReq = await _context.Ocandreqs.AsNoTracking()
                        .Where(r => r.Id == pedimento.IdReq)
                        .Select(r => r.IdReference).FirstOrDefaultAsync();
                    if (idBranchReq > 0)
                    {
                        var connCo = _context.Database.GetDbConnection();
                        if (connCo.State != System.Data.ConnectionState.Open) await connCo.OpenAsync();
                        using var cmdCo = connCo.CreateCommand();
                        cmdCo.CommandText = "SELECT id_company FROM smp.dbo.Branchs WHERE id = @id";
                        var pCo = cmdCo.CreateParameter(); pCo.ParameterName = "@id"; pCo.Value = idBranchReq;
                        cmdCo.Parameters.Add(pCo);
                        var res = await cmdCo.ExecuteScalarAsync();
                        if (res != null && res != System.DBNull.Value) idCompany = System.Convert.ToInt32(res);
                    }
                }

                if (idCompany == 0) idCompany = pedimento.IdRoot ?? 0;

                // Catálogo de condiciones de pago de la empresa (para calcular Anticipo OC).
                var condicionPagoMap = await _context.CondicionesPago
                    .AsNoTracking()
                    .Where(cp => cp.IdCompany == idCompany)
                    .ToDictionaryAsync(cp => cp.Id);

                // IVA% de la sucursal de la requisición (setup almacén). El precio guardado es BASE
                // (Opción B), así que el Total x OC aplica el IVA por línea (mas_iva).
                var idBranchPed = await _context.Ocandreqs
                    .Where(r => r.Id == pedimento.IdReq)
                    .Select(r => r.IdReference)
                    .FirstOrDefaultAsync();
                var ivaPercentPed = await _context.Setups
                    .Where(s => s.IdBranch == idBranchPed && s.Active)
                    .Select(s => s.Iva)
                    .FirstOrDefaultAsync() ?? 0m;
                var ivaFactorPed = 1m + ivaPercentPed / 100m;

                // OCs de la requisición padre que tengan alguno de esos proveedores
                var ocs = await _context.Ocandreqs
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
                        o.NumCotizacion,
                        o.CondicionesPago,
                        o.VigenciaCotizacion,
                        o.IdCondicionPago,
                        o.AnticipoPagado,
                        o.AnticipoMonto,
                        o.FechaAnticipo,
                        o.AnticipoEstado,
                        Total = _context.Detailsreqoc
                            .Where(d => d.IdMovement == o.Id && d.Active == true)
                            .Sum(d => (decimal?)(Math.Round(d.Price * (d.MasIva == true ? ivaFactorPed : 1m), 2) * d.Quantity)) ?? 0m,
                        countrow = _context.Detailsreqoc
                            .Count(d => d.IdMovement == o.Id && d.Active == true),
                        // Moneda REAL de la OC = la de sus ítems (un solo proveedor → una sola moneda).
                        // La cabecera o.IdCurrency suele venir 0/sin poblar, por eso se toma del primer ítem.
                        ItemCurrency = _context.Detailsreqoc
                            .Where(d => d.IdMovement == o.Id && d.Active == true)
                            .Select(d => d.IdCurrency)
                            .FirstOrDefault()
                    })
                    .AsNoTracking()
                    .ToListAsync();

                // ── Base del Anticipo OC por orden ──
                // Items normales: precio(round2 con IVA si mas_iva) × cantidad_OC.
                // Items "COMPRA AUTORIZADA SIN LIMITE" (cantidad_OC = 0): se usa la cantidad de la
                // REQUISICIÓN padre (mismo material) × precio, para que el anticipo no salga en 0.
                var ocIdsAnt  = ocs.Select(o => o.Id).ToList();
                var reqIdsAnt = ocs.Where(o => o.IdReq.HasValue).Select(o => o.IdReq!.Value).Distinct().ToList();

                var ocItemsAnt = await _context.Detailsreqoc.AsNoTracking()
                    .Where(d => ocIdsAnt.Contains(d.IdMovement) && d.Active == true)
                    .Select(d => new { d.IdMovement, d.IdSupplie, d.TypeOc, d.Price, d.MasIva, d.Quantity })
                    .ToListAsync();

                var reqQtyMap = reqIdsAnt.Count > 0
                    ? (await _context.Detailsreqoc.AsNoTracking()
                        .Where(d => reqIdsAnt.Contains(d.IdMovement) && d.Active == true)
                        .GroupBy(d => new { d.IdMovement, d.IdSupplie })
                        .Select(g => new { g.Key.IdMovement, g.Key.IdSupplie, Qty = g.Sum(x => x.Quantity) })
                        .ToListAsync())
                        .ToDictionary(x => (x.IdMovement, x.IdSupplie), x => x.Qty)
                    : new Dictionary<(int, int), decimal>();

                var anticipoBaseByOc = new Dictionary<int, decimal>();
                foreach (var o in ocs)
                {
                    decimal baseAnt = 0m;
                    foreach (var it in ocItemsAnt.Where(i => i.IdMovement == o.Id))
                    {
                        var unit = Math.Round(it.Price * (it.MasIva == true ? ivaFactorPed : 1m), 2);
                        decimal qty;
                        if (string.Equals(it.TypeOc, "COMPRA AUTORIZADA SIN LIMITE", StringComparison.OrdinalIgnoreCase))
                            qty = (o.IdReq.HasValue && reqQtyMap.TryGetValue((o.IdReq.Value, it.IdSupplie), out var rq)) ? rq : 0m;
                        else
                            qty = it.Quantity;
                        baseAnt += unit * qty;
                    }
                    anticipoBaseByOc[o.Id] = baseAnt;
                }

                // ── Total x OC REAL = Σ por ítem de Σ por entrega: round2(precio × IVA de la entrega) ×
                // (cantidad recibida en almacén si entró, o la planeada). Cuadra con Nivel 4/5. Ítems sin
                // entregas (no usan el mecanismo) caen a precio × cantidad con el IVA del ítem.
                var allItemsTot = await _context.Detailsreqoc.AsNoTracking()
                    .Where(d => ocIdsAnt.Contains(d.IdMovement) && d.Active == true)
                    .Select(d => new { d.Id, d.IdMovement, d.Price, d.MasIva, d.Quantity })
                    .ToListAsync();
                var itemIdsTot = allItemsTot.Select(i => i.Id).ToList();
                var entregasTot = itemIdsTot.Count > 0
                    ? await _context.EntregasOc.AsNoTracking()
                        .Where(g => itemIdsTot.Contains(g.IdDetailsreqoc) && g.Active)
                        .Select(g => new { g.Id, g.IdDetailsreqoc, g.MasIva, g.CantidadRecibir })
                        .ToListAsync()
                    : new();
                var entregaIdsTot = entregasTot.Select(g => g.Id).ToList();
                var almacenByEntregaTot = entregaIdsTot.Count > 0
                    ? (await _context.EntradasMolienda.AsNoTracking()
                        .Where(e => e.IdEntrega != null && entregaIdsTot.Contains(e.IdEntrega.Value) && e.Active)
                        .GroupBy(e => e.IdEntrega!.Value)
                        .Select(g => new { Entrega = g.Key, Qty = g.Sum(x => x.CantidadEntrada ?? 0m) })
                        .ToListAsync())
                        .ToDictionary(x => x.Entrega, x => x.Qty)
                    : new Dictionary<int, decimal>();

                var realTotalByOc = new Dictionary<int, decimal>();
                foreach (var o in ocs)
                {
                    decimal ocTotal = 0m;
                    foreach (var it in allItemsTot.Where(i => i.IdMovement == o.Id))
                    {
                        var itemEntregas = entregasTot.Where(g => g.IdDetailsreqoc == it.Id).ToList();
                        if (itemEntregas.Count > 0)
                        {
                            foreach (var g in itemEntregas)
                            {
                                var factor = g.MasIva ? ivaFactorPed : 1m;
                                var unit = Math.Round(it.Price * factor, 2);
                                var qa = almacenByEntregaTot.TryGetValue(g.Id, out var q) ? q : 0m;
                                var qty = qa > 0 ? qa : (g.CantidadRecibir ?? 0m);
                                ocTotal += unit * qty;
                            }
                        }
                        else
                        {
                            var factor = (it.MasIva == true) ? ivaFactorPed : 1m;
                            var unit = Math.Round(it.Price * factor, 2);
                            ocTotal += unit * it.Quantity;
                        }
                    }
                    realTotalByOc[o.Id] = ocTotal;
                }

                // Mezcla en memoria: cuando la COTIZ hermana tenga los campos, sobreescriben al OC.
                var result = ocs.Select(o =>
                {
                    cotizByProvider.TryGetValue(o.IdProvider, out var c);

                    // Resolver FK de condición de pago: COTIZ tiene prioridad sobre OC.
                    var idCondPago = c?.IdCondicionPago ?? o.IdCondicionPago;

                    // Calcular Anticipo OC = base × cantidad / 100 si calculo_anticipo = true.
                    // La base maneja el caso SIN LIMITE (cantidad de la REQUIS × precio).
                    decimal anticipoOc = 0m;
                    if (idCondPago.HasValue
                        && condicionPagoMap.TryGetValue(idCondPago.Value, out var cp)
                        && cp.CalculoAnticipo)
                    {
                        var baseAnt = anticipoBaseByOc.TryGetValue(o.Id, out var ba) ? ba : o.Total;
                        anticipoOc = baseAnt * cp.Cantidad / 100m;
                    }

                    return new
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
                        // Moneda de la OC tomada de sus ítems (Opción A). Si no hay ítems, cae al header.
                        IdCurrency = o.ItemCurrency ?? o.IdCurrency,
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
                        NumCotizacion = !string.IsNullOrEmpty(c?.NumCotizacion) ? c.NumCotizacion : o.NumCotizacion,
                        CondicionesPago = !string.IsNullOrEmpty(c?.CondicionesPago) ? c.CondicionesPago : o.CondicionesPago,
                        VigenciaCotizacion = c?.VigenciaCotizacion ?? o.VigenciaCotizacion,
                        IdCondicionPago = idCondPago,
                        AnticipoOc = anticipoOc,
                        o.AnticipoPagado,
                        o.AnticipoMonto,
                        o.FechaAnticipo,
                        o.AnticipoEstado,
                        // Total x OC real (por entrega: IVA + cantidad de almacén). Fallback al subquery si faltara.
                        Total = realTotalByOc.TryGetValue(o.Id, out var rtot) ? rtot : o.Total,
                        o.countrow
                    };
                }).Cast<object>().ToList();

                return result;
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
        Task<List<object>> GetCompraRapidaItems(int idBranch, int idMaterial = 0);
        Task<object> SyncCompraRapida(int idReq);
        Task<bool> CompraRapidaHasEntradas(int idItem);
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
