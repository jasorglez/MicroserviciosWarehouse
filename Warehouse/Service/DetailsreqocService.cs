
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehouse.Models;
using Warehouse.Models.DTOs;

namespace Warehouse.Service
{
    public class DetailsreqocService : IDetailsreqocService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<DetailsreqocService> _logger;

        public DetailsreqocService(DbWarehouseContext context, ILogger<DetailsreqocService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<object>> GetDetails(int idMovement)
        {
            try
            {
                return await _context.Detailsreqoc
                    .Where(d => d.Active == true && idMovement == d.IdMovement)
                    .GroupJoin(_context.Materials,
                        d => d.IdSupplie,
                        m => m.Id,
                        (d, materials) => new { Details = d, Materials = materials })
                    .SelectMany(
                        dm => dm.Materials.DefaultIfEmpty(),
                        (dm, m) => new { dm.Details, Material = m })
                    .GroupJoin(_context.Catalogs,
                        dm => dm.Material != null ? dm.Material.IdMedida : (int?)null,
                        c => c.Id,
                        (dm, catalogs) => new { dm.Details, dm.Material, Catalogs = catalogs })
                    .SelectMany(
                        dmc => dmc.Catalogs.DefaultIfEmpty(),
                        (dmc, c) => new
                        {
                            id = dmc.Details.Id,
                            recurrent = dmc.Details.Recurrent,
                            observation = dmc.Details.Observation,
                            idMovement = dmc.Details.IdMovement,
                            idSupplie = dmc.Details.IdSupplie,
                            code = dmc.Material != null ? dmc.Material.Insumo : string.Empty,
                            description = dmc.Material != null ? dmc.Material.Description : string.Empty,
                            measure = c != null ? c.Description : string.Empty,
                            quantity = dmc.Details.Quantity,
                            price = dmc.Details.Price,
                            total = dmc.Details.Total,
                            intorext = dmc.Details.Intorext,
                            type = dmc.Details.Type,
                            idProvider = dmc.Details.IdProvider,
                            nameProvider = dmc.Details.NameProvider,
                            descriptionNewArticle = dmc.Details.DescriptionNewArticle,
                            urlNewArticle = dmc.Details.UrlNewArticle,
                            justificationNewArticle = dmc.Details.JustificationNewArticle,
                            comment = dmc.Details.Comment,
                            dateuse = dmc.Details.Dateuse,
                            pedimento = dmc.Details.Pedimento,
                            pedimentoNum = dmc.Details.PedimentoNum,
                            namearticle = dmc.Details.NameArticle,
                            numarticle = dmc.Details.NumArticle,
                            typePriority = dmc.Details.TypePriority,
                            tiempoEntrega = dmc.Details.TiempoEntrega,
                            compraMinima = dmc.Details.CompraMinima,
                            autorizado = dmc.Details.Autorizado,
                            active = dmc.Details.Active,
                            typeoc = dmc.Details.TypeOc,
                            datepostpone = dmc.Details.DatePostpone,
                            caducidad = dmc.Details.Caducidad,
                            compraRapida = dmc.Details.CompraRapida
                        })
                    .AsNoTracking()
                    .ToListAsync<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Details for movement {IdMovement}", idMovement);
                throw;
            }
        }

        // Query Syntax (Alternative)
        public async Task<List<PurchaseOrderDetail>> GetPurchaseOrderDetailsQuery(int idProv)
        {

            var result = await (
                from o in _context.Ocandreqs
                join d in _context.Detailsreqoc on o.Id equals d.IdMovement
                join m in _context.Materials on d.IdSupplie equals m.Id
                where o.Type == "OC" && o.IdProvider==idProv && d.Active == true 
                orderby o.Id
                select new PurchaseOrderDetail
                {
                    Id           = o.Id,
                    Folio        = o.Folio,
                    DateCreate   = o.DateCreate,
                    IdSupplie    = d.IdSupplie,
                    Description  = m.Description,
                    Quantity     = d.Quantity,
                    Price        = d.Price,
                    IdProvider   = o.IdProvider 
                })
                .AsNoTracking()
                .ToListAsync();

            return result;
        }

        public async Task<Detailsreqoc?> Save(Detailsreqoc detail)
        {
            try
            {
                _context.Detailsreqoc.Add(detail);
                await _context.SaveChangesAsync();

                // ✅ Actualizar DateModified de la requisición padre
                await UpdateParentRequisitionModified(detail.IdMovement);

                return detail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Detail");
                throw;
            }
        }

        public async Task SaveBulk(List<Detailsreqoc> details)
        {
            try
            {
                _context.Detailsreqoc.AddRange(details);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving bulk Details");
                throw;
            }
        }

        public async Task<Detailsreqoc?> Update(int id, Detailsreqoc detail)
        {
            var existingItem = await _context.Detailsreqoc.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to update non-existent Detail with ID {Id}", id);
                return null;
            }

            try
            {
                detail.Id = id;
                _context.Entry(existingItem).CurrentValues.SetValues(detail);
                await _context.SaveChangesAsync();

                // ✅ Actualizar DateModified de la requisición padre
                await UpdateParentRequisitionModified(existingItem.IdMovement);

                return existingItem;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating Detail with ID {Id}", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Detail with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            var existingItem = await _context.Detailsreqoc.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to delete non-existent Detail with ID {Id}", id);
                return false;
            }

            try
            {
                existingItem.Active = false;
                await _context.SaveChangesAsync();

                // ✅ Actualizar DateModified de la requisición padre
                await UpdateParentRequisitionModified(existingItem.IdMovement);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Detail with ID {Id}", id);
                throw;
            }
        }

        public async Task<FrequentArticlesResponse> GetFrequentArticles(string solicit, int idDepartment, int idBranch)
        {
            try
            {
                // Calcular el total de requisiciones que tienen artículos registrados
                var totalRequisitions = await _context.Detailsreqoc
                    .Where(d => d.Active == true)
                    .Join(_context.Ocandreqs,
                        d => d.IdMovement,
                        o => o.Id,
                        (d, o) => new { Detail = d, Order = o })
                    .Where(x => x.Order.Solicit == solicit
                        && x.Order.IdDepartament == idDepartment
                        && x.Order.IdReference == idBranch
                        && x.Order.Active == true
                        && x.Order.Type == "REQUIS")
                    .Select(x => x.Order.Id)
                    .Distinct()
                    .CountAsync();

                // Obtener los TOP 3 artículos más solicitados
                var frequentArticles = await _context.Detailsreqoc
                    .Where(d => d.Active == true)
                    .Join(_context.Ocandreqs,
                        d => d.IdMovement,
                        o => o.Id,
                        (d, o) => new { Detail = d, Order = o })
                    .Where(x => x.Order.Solicit == solicit
                        && x.Order.IdDepartament == idDepartment
                        && x.Order.IdReference == idBranch
                        && x.Order.Active == true
                        && x.Order.Type == "REQUIS")
                    .GroupBy(x => new { x.Detail.IdSupplie, x.Detail.NameArticle })
                    .Select(g => new FrequentArticleDto
                    {
                        IdSupplie = g.Key.IdSupplie,
                        NameArticle = g.Key.NameArticle ?? string.Empty,
                        CountRequested = g.Count(),
                        TotalQuantity = g.Sum(x => x.Detail.Quantity)
                    })
                    .OrderByDescending(x => x.CountRequested)
                    .Take(3)
                    .AsNoTracking()
                    .ToListAsync();

                return new FrequentArticlesResponse
                {
                    Articles = frequentArticles,
                    TotalRequisitions = totalRequisitions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving frequent articles for solicit {Solicit}, department {IdDepartment}, branch {IdBranch}",
                    solicit, idDepartment, idBranch);
                throw;
            }
        }

        public async Task PatchTypeOc(int id, string typeOc)
        {
            var item = await _context.Detailsreqoc.FindAsync(id);
            if (item == null) return;
            item.TypeOc = typeOc;
            await _context.SaveChangesAsync();
        }

        public async Task PatchCantidadConceptualizada(int id, decimal cantidad)
        {
            var item = await _context.Detailsreqoc.FindAsync(id);
            if (item == null) return;
            item.CantidadConceptualizada = cantidad;
            await _context.SaveChangesAsync();
        }

        public async Task SyncObservationBySupplieAndProvider(int idSupplie, int idProvider, string observation)
        {
            try
            {
                var rows = await _context.Detailsreqoc
                    .Where(d => d.Active == true && d.IdSupplie == idSupplie && d.IdProvider == idProvider)
                    .ToListAsync();

                foreach (var row in rows)
                {
                    row.Observation = observation;
                }

                if (rows.Count > 0)
                    await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing observation for supplie {IdSupplie} provider {IdProvider}", idSupplie, idProvider);
                throw;
            }
        }

        // ✅ Método privado para actualizar DateModified de la requisición padre (y del abuelo si es COTIZ)
        private async Task UpdateParentRequisitionModified(int idMovement)
        {
            try
            {
                var parentDocument = await _context.Ocandreqs.FindAsync(idMovement);
                if (parentDocument != null)
                {
                    // Actualizar el documento directo (COTIZ o REQUIS)
                    parentDocument.DateModified = DateTime.Now;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Updated DateModified for document {IdMovement}", idMovement);

                    // Si es una COTIZ, también actualizar el REQUIS padre
                    if (parentDocument.Type == "COTIZ" && parentDocument.IdReq.HasValue && parentDocument.IdReq > 0)
                    {
                        var grandparentRequisition = await _context.Ocandreqs.FindAsync(parentDocument.IdReq);
                        if (grandparentRequisition != null)
                        {
                            grandparentRequisition.DateModified = DateTime.Now;
                            await _context.SaveChangesAsync();
                            _logger.LogInformation("Updated DateModified for parent requisition {IdReq}", parentDocument.IdReq);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating DateModified for document {IdMovement}", idMovement);
                // No lanzar excepción para no bloquear la operación principal
            }
        }
    }

    public interface IDetailsreqocService
    {
        Task<List<object>> GetDetails(int idMovement);
        Task<List<PurchaseOrderDetail>> GetPurchaseOrderDetailsQuery(int idProv);
        Task<Detailsreqoc?> Save(Detailsreqoc detail);
        Task SaveBulk(List<Detailsreqoc> details);
        Task<Detailsreqoc?> Update(int id, Detailsreqoc detail);
        Task<bool> Delete(int id);
        Task<FrequentArticlesResponse> GetFrequentArticles(string solicit, int idDepartment, int idBranch);
        Task PatchTypeOc(int id, string typeOc);
        Task PatchCantidadConceptualizada(int id, decimal cantidad);
        Task SyncObservationBySupplieAndProvider(int idSupplie, int idProvider, string observation);
    }
}