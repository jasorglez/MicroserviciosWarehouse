using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse.Service;

    public class DetailsinandoutService : IDetailsinandoutService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<DetailsinandoutService> _logger;

        private static bool IsKilogramUnit(string? measure)
        {
            var normalized = (measure ?? string.Empty).Trim().ToUpperInvariant();
            return normalized is "KG" or "KGS" or "KILO" or "KILOS" or "KILOGRAMO" or "KILOGRAMOS";
        }

        private static bool IsMeterUnit(string? measure)
        {
            var normalized = (measure ?? string.Empty).Trim();
            return normalized.Equals("m", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("metro", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("metros", StringComparison.OrdinalIgnoreCase);
        }

        public DetailsinandoutService(DbWarehouseContext context, ILogger<DetailsinandoutService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task ApplyMaterialBrandAsync(Detailsinandout detail, bool isNew)
        {
            if (!detail.IdProduct.HasValue) return;

            var companyId = await _context.Materials.AsNoTracking()
                .Where(material => material.Id == detail.IdProduct.Value)
                .Select(material => material.IdCompany)
                .FirstOrDefaultAsync();

            var configured = await (from relation in _context.MaterialBrands.AsNoTracking()
                                    join catalog in _context.Catalogs.AsNoTracking() on relation.IdCatalog equals catalog.Id
                                    where relation.IdMaterial == detail.IdProduct.Value && relation.Active
                                        && catalog.IdCompany == companyId && catalog.Type == "BRAND" && catalog.Active == 1
                                    orderby catalog.Description
                                    select new { relation.IdCatalog, catalog.Description })
                .ToListAsync();

            if (detail.IdBrand.HasValue)
            {
                var selected = await (from relation in _context.MaterialBrands.AsNoTracking()
                                      join catalog in _context.Catalogs.AsNoTracking() on relation.IdCatalog equals catalog.Id
                                      where relation.IdMaterial == detail.IdProduct.Value && relation.IdCatalog == detail.IdBrand.Value
                                          && catalog.IdCompany == companyId && catalog.Type == "BRAND"
                                      select new { RelationActive = relation.Active, CatalogActive = catalog.Active, catalog.Description }).FirstOrDefaultAsync();
                if (selected == null || (isNew && (!selected.RelationActive || selected.CatalogActive != 1)))
                    throw new InvalidOperationException("La marca seleccionada no está configurada para este material.");
                detail.BrandName ??= selected.Description;
            }
            else if (configured.Count == 1)
            {
                detail.IdBrand = configured[0].IdCatalog;
                detail.BrandName = configured[0].Description;
            }
            else if (configured.Count > 1 && isNew && detail.BrandName != "Sin marca")
            {
                throw new InvalidOperationException("Selecciona una marca para este material antes de guardar el movimiento.");
            }
        }

        private async Task ValidateBrandStockAsync(Detailsinandout detail)
        {
            if (!detail.IdProduct.HasValue || detail.Quantity is null or <= 0) return;
            var movement = await _context.Inandouts.AsNoTracking()
                .Where(item => item.Id == detail.IdInandout && item.Active == true)
                .Select(item => new { item.Type, item.IdWarehouse })
                .FirstOrDefaultAsync();
            if (movement?.Type != "OUT" || !movement.IdWarehouse.HasValue) return;

            var multiBrand = await _context.MaterialBrands.AsNoTracking()
                .CountAsync(brand => brand.IdMaterial == detail.IdProduct.Value && brand.Active);
            if (multiBrand < 2) return;

            var available = await _context.Detailsinandout.AsNoTracking()
                .Where(item => item.Active && item.IdProduct == detail.IdProduct && item.IdBrand == detail.IdBrand)
                .Join(_context.Inandouts.AsNoTracking(), item => item.IdInandout, header => header.Id,
                    (item, header) => new { item, header })
                .Where(pair => pair.header.Active != false && pair.header.IdWarehouse == movement.IdWarehouse
                    && (pair.header.Type == "IN" || pair.header.Type == "OUT"))
                .SumAsync(pair => (decimal?)(pair.header.Type == "IN" ? pair.item.Quantity : -pair.item.Quantity)) ?? 0m;

            if (detail.Id > 0)
            {
                var existing = await _context.Detailsinandout.AsNoTracking()
                    .Where(item => item.Id == detail.Id && item.Active)
                    .Select(item => new { item.IdProduct, item.IdBrand, item.Quantity })
                    .FirstOrDefaultAsync();
                if (existing?.IdProduct == detail.IdProduct && existing.IdBrand == detail.IdBrand)
                    available += existing.Quantity ?? 0m;
            }

            if (detail.Quantity.Value > Math.Max(0m, available))
                throw new InvalidOperationException($"La existencia de la marca seleccionada es {Math.Max(0m, available)}; no puedes retirar {detail.Quantity.Value}.");
        }

        public async Task<List<object>> GetDetails(int idInandout)
        {
            try
            {
                var details = await _context.Detailsinandout
                    .AsNoTracking()
                    .Where(d => d.Active == true && d.IdInandout == idInandout)
                    .Select(detail => new
                    {
                        detail.Id,
                        detail.IdInandout,
                        detail.IdProduct,
                        detail.IdBrand,
                        detail.BrandName,
                        detail.IdParentProduct,
                        detail.ComponentName,
                        detail.Quantity,
                        detail.Pending,
                        detail.Total,
                        detail.SourceQuantity,
                        detail.SourceMeasure,
                        detail.SourceMeasureId,
                        detail.GramsPerMeterAtMovement,
                        detail.ConversionFactorAtMovement,
                        detail.TargetMeasureAtMovement,
                        detail.Type,
                        detail.Active
                    })
                    .ToListAsync();

                var productIds = details
                    .Where(detail => detail.IdProduct.HasValue)
                    .Select(detail => detail.IdProduct!.Value)
                    .Distinct()
                    .ToArray();
                var materials = new List<(int Id, string? Code, string? Description, int? MeasureId)>();
                if (productIds.Length > 0)
                {
                    var materialRows = await _context.Materials.AsNoTracking()
                        .Where(material => productIds.Contains(material.Id))
                        .Select(material => new
                        {
                            material.Id,
                            Code = material.Insumo,
                            material.Description,
                            MeasureId = material.IdMedida
                        })
                        .ToListAsync();
                    materials = materialRows
                        .Select(material => (material.Id, material.Code, material.Description, material.MeasureId))
                        .ToList();
                }

                var materialById = materials.ToDictionary(material => material.Id);
                var measureIds = materials
                    .Where(material => material.MeasureId.HasValue)
                    .Select(material => material.MeasureId!.Value)
                    .Distinct()
                    .ToArray();
                var measures = measureIds.Length == 0
                    ? new Dictionary<int, string>()
                    : await _context.Catalogs.AsNoTracking()
                        .Where(catalog => measureIds.Contains(catalog.Id)
                            && catalog.Type == "MEASURE" && catalog.Active == 1)
                        .Select(catalog => new { catalog.Id, catalog.Description })
                        .ToDictionaryAsync(catalog => catalog.Id, catalog => catalog.Description);

                return details.Select(detail =>
                {
                    var hasMaterial = materialById.TryGetValue(detail.IdProduct ?? 0, out var material);
                    var measure = hasMaterial && material.MeasureId is int measureId && measures.TryGetValue(measureId, out var description)
                        ? description
                        : string.Empty;

                    return (object)new
                    {
                        detail.Id,
                        code = hasMaterial ? material.Code : null,
                        detail.IdInandout,
                        detail.IdProduct,
                        detail.IdParentProduct,
                        description = detail.ComponentName ?? (hasMaterial ? material.Description : null) ?? string.Empty,
                        Quantity = detail.SourceQuantity ?? detail.Quantity,
                        detail.IdBrand,
                        brand = detail.BrandName,
                        detail.Pending,
                        detail.Total,
                        measure = detail.SourceMeasure
                            ?? detail.TargetMeasureAtMovement
                            ?? (detail.GramsPerMeterAtMovement > 0 ? "m" : measure),
                        detail.SourceQuantity,
                        detail.SourceMeasure,
                        detail.SourceMeasureId,
                        detail.GramsPerMeterAtMovement,
                        detail.ConversionFactorAtMovement,
                        detail.TargetMeasureAtMovement,
                        detail.Type,
                        detail.Active
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

    public async Task<object> GetStockByProductAndWarehouse(int productId, int warehouseId, int? brandId = null)
    {
        try
        {
            var stock = await _context.Detailsinandout.AsNoTracking()
                .Where(detail => detail.Active && detail.IdProduct == productId
                    && (!brandId.HasValue || (brandId.Value == 0 ? !detail.IdBrand.HasValue : detail.IdBrand == brandId.Value)))
                .Join(_context.Inandouts.AsNoTracking(),
                    detail => detail.IdInandout,
                    movement => movement.Id,
                    (detail, movement) => new { detail, movement })
                .Where(item => item.movement.Active != false
                    && item.movement.IdWarehouse == warehouseId
                    && (item.movement.Type == "IN" || item.movement.Type == "OUT"))
                .SumAsync(item => (decimal?)(item.movement.Type == "IN"
                    ? item.detail.Quantity
                    : -item.detail.Quantity)) ?? 0m;

            return Math.Max(0m, stock);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stock for product {ProductId} at warehouse {WarehouseId}", productId, warehouseId);
            throw;
        }
    }

    public async Task<object> GetTotalInByProductAndWarehouse(int productId, int warehouseId, int? brandId = null)
    {
        try
        {
            var total = await _context.Detailsinandout
                .Where(d => d.Active == true && d.IdProduct == productId
                    && (!brandId.HasValue || (brandId.Value == 0 ? !d.IdBrand.HasValue : d.IdBrand == brandId.Value)))
                .Join(_context.Inandouts,
                    d => d.IdInandout,
                    i => i.Id,
                    (d, i) => new { d, i })
                .Join(_context.Warehouses,
                    di => di.i.IdWarehouse,
                    w => w.Id,
                    (di, w) => new { di.d, di.i, w })
                .Where(x => x.i.Type == "IN" && x.w.Id == warehouseId)
                .SumAsync(x => (decimal?)x.d.Quantity) ?? 0;

            return total;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving total IN quantity for product {ProductId} at warehouse {WarehouseId}", productId, warehouseId);
            throw;
        }
    }


    private async Task SaveDetailRowsWithTransferAsync(List<Detailsinandout> details)
    {
        if (details.Count == 0) return;
        var pairedMovementId = await _context.Inandouts.AsNoTracking()
            .Where(movement => movement.Id == details[0].IdInandout && movement.Active == true)
            .Select(movement => movement.IdTransferPair)
            .FirstOrDefaultAsync();

        await using var transaction = await _context.Database.BeginTransactionAsync();
        _context.Detailsinandout.AddRange(details);
        await _context.SaveChangesAsync();
        if (pairedMovementId.HasValue)
        {
            var pairedDetails = details.Select(source => new Detailsinandout
            {
                IdInandout = pairedMovementId.Value,
                IdTransferPair = source.Id,
                IdProduct = source.IdProduct,
                IdBrand = source.IdBrand,
                BrandName = source.BrandName,
                IdParentProduct = source.IdParentProduct,
                ComponentName = source.ComponentName,
                SourceQuantity = source.SourceQuantity,
                SourceMeasure = source.SourceMeasure,
                SourceMeasureId = source.SourceMeasureId,
                GramsPerMeterAtMovement = source.GramsPerMeterAtMovement,
                ConversionFactorAtMovement = source.ConversionFactorAtMovement,
                TargetMeasureAtMovement = source.TargetMeasureAtMovement,
                TargetMeasureIdAtMovement = source.TargetMeasureIdAtMovement,
                Quantity = source.Quantity,
                Pending = source.Pending,
                Total = source.Total,
                Type = source.Type,
                Active = source.Active
            }).ToList();
            _context.Detailsinandout.AddRange(pairedDetails);
            await _context.SaveChangesAsync();
            for (var index = 0; index < details.Count; index++)
                details[index].IdTransferPair = pairedDetails[index].Id;
            await _context.SaveChangesAsync();
        }
        await transaction.CommitAsync();
    }

    private async Task SyncPairedDetailAsync(Detailsinandout source)
    {
        if (!source.IdTransferPair.HasValue) return;
        var paired = await _context.Detailsinandout.FindAsync(source.IdTransferPair.Value);
        if (paired == null) return;
        paired.IdProduct = source.IdProduct;
        paired.IdParentProduct = source.IdParentProduct;
        paired.ComponentName = source.ComponentName;
        paired.SourceQuantity = source.SourceQuantity;
        paired.SourceMeasure = source.SourceMeasure;
        paired.SourceMeasureId = source.SourceMeasureId;
        paired.GramsPerMeterAtMovement = source.GramsPerMeterAtMovement;
        paired.ConversionFactorAtMovement = source.ConversionFactorAtMovement;
        paired.TargetMeasureAtMovement = source.TargetMeasureAtMovement;
        paired.TargetMeasureIdAtMovement = source.TargetMeasureIdAtMovement;
        paired.Quantity = source.Quantity;
        paired.Pending = source.Pending;
        paired.Total = source.Total;
        paired.Type = source.Type;
        paired.Active = source.Active;
        await _context.SaveChangesAsync();
    }

    public async Task Save(Detailsinandout detail)
        {
            try
            {
                await ApplyMaterialBrandAsync(detail, isNew: detail.Id <= 0);
                var movementType = await _context.Inandouts.AsNoTracking()
                    .Where(movement => movement.Id == detail.IdInandout && movement.Active == true)
                    .Select(movement => movement.Type)
                    .FirstOrDefaultAsync();
                if (movementType == "IN")
                {
                    var material = detail.IdProduct.HasValue
                        ? await _context.Materials.AsNoTracking()
                            .Where(item => item.Id == detail.IdProduct.Value)
                            .Select(item => new { item.GramsPerMeter, item.IdMedida, item.IdCompany })
                            .FirstOrDefaultAsync()
                        : null;
                    var purchaseMeasure = material?.IdMedida.HasValue == true
                        ? await _context.Catalogs.AsNoTracking()
                            .Where(unit => unit.Id == material.IdMedida.Value)
                            .Select(unit => unit.Description)
                            .FirstOrDefaultAsync()
                        : null;
                    var inventoryQuantity = detail.Quantity;
                    var sourceMeasureId = detail.SourceMeasureId ?? material?.IdMedida;
                    var conversion = material != null && sourceMeasureId.HasValue
                        ? await _context.MaterialConversions.AsNoTracking()
                            .Where(item => item.IdCompany == material.IdCompany && item.IdMaterial == detail.IdProduct
                                && item.IdMeasureFrom == sourceMeasureId.Value && item.Active)
                            .Select(item => new { item.Factor, item.IdMeasureTo })
                            .FirstOrDefaultAsync()
                        : null;
                    if (conversion != null && detail.Quantity is > 0)
                    {
                        var targetMeasure = await _context.Catalogs.AsNoTracking()
                            .Where(unit => unit.Id == conversion.IdMeasureTo)
                            .Select(unit => unit.Description)
                            .FirstOrDefaultAsync();
                        detail.SourceQuantity = detail.Quantity;
                        detail.SourceMeasureId = sourceMeasureId;
                        detail.SourceMeasure = await _context.Catalogs.AsNoTracking()
                            .Where(unit => unit.Id == sourceMeasureId.Value)
                            .Select(unit => unit.Description)
                            .FirstOrDefaultAsync();
                        detail.ConversionFactorAtMovement = conversion.Factor;
                        detail.TargetMeasureAtMovement = targetMeasure;
                        detail.TargetMeasureIdAtMovement = conversion.IdMeasureTo;
                        inventoryQuantity = detail.Quantity.Value * conversion.Factor;
                    }
                    else if (material?.GramsPerMeter is > 0 && detail.Quantity is > 0)
                    {
                        if (!IsKilogramUnit(purchaseMeasure))
                            throw new InvalidOperationException("El material con conversión gramos/metro debe tener unidad de compra en kilogramos.");
                        detail.SourceQuantity = detail.Quantity;
                        detail.SourceMeasure = purchaseMeasure ?? "kg";
                        detail.GramsPerMeterAtMovement = material.GramsPerMeter.Value;
                        inventoryQuantity = detail.Quantity.Value * 1000m / material.GramsPerMeter.Value;
                    }

                    var components = await _context.MaterialComponents.AsNoTracking()
                        .Where(component => component.IdParentMaterial == detail.IdProduct && component.Active)
                        .ToListAsync();
                    if (components.Count > 0)
                    {
                        if (detail.Quantity is not > 0)
                            throw new InvalidOperationException("La cantidad recibida debe ser mayor que cero para desglosar el material.");

                        var componentDetails = components.Select(component => new Detailsinandout
                        {
                            IdInandout = detail.IdInandout,
                            IdProduct = null,
                            IdParentProduct = detail.IdProduct,
                            ComponentName = component.ComponentName,
                            Quantity = inventoryQuantity!.Value * component.Quantity,
                            Pending = detail.Pending * component.Quantity,
                            Total = detail.Total * component.Quantity,
                            Type = detail.Type,
                            Active = detail.Active,
                        }).ToList();
                        await SaveDetailRowsWithTransferAsync(componentDetails);
                        return;
                    }

                    if (detail.SourceQuantity.HasValue)
                        detail.Quantity = inventoryQuantity;
                }
                else if (movementType == "OUT" && detail.IdProduct.HasValue)
                {
                    await ValidateBrandStockAsync(detail);
                    var productCompany = await _context.Materials.AsNoTracking()
                        .Where(item => item.Id == detail.IdProduct.Value)
                        .Select(item => item.IdCompany)
                        .FirstOrDefaultAsync();
                    var purchaseMeasureId = await _context.Materials.AsNoTracking()
                        .Where(item => item.Id == detail.IdProduct.Value)
                        .Select(item => item.IdMedida)
                        .FirstOrDefaultAsync();
                    var conversion = await _context.MaterialConversions.AsNoTracking()
                        .Where(item => item.IdCompany == productCompany && item.IdMaterial == detail.IdProduct.Value
                            && item.IdMeasureFrom == purchaseMeasureId && item.Active)
                        .OrderBy(item => item.Id)
                        .Select(item => new { item.Factor, item.IdMeasureTo })
                        .FirstOrDefaultAsync();
                    if (conversion != null)
                    {
                        detail.TargetMeasureAtMovement = await _context.Catalogs.AsNoTracking()
                            .Where(unit => unit.Id == conversion.IdMeasureTo)
                            .Select(unit => unit.Description)
                            .FirstOrDefaultAsync();
                        detail.SourceQuantity = detail.Quantity;
                        detail.SourceMeasure = detail.TargetMeasureAtMovement;
                        detail.TargetMeasureIdAtMovement = conversion.IdMeasureTo;
                        detail.SourceMeasureId = conversion.IdMeasureTo;
                        detail.ConversionFactorAtMovement = 1m;
                    }
                    var gramsPerMeter = await _context.Materials.AsNoTracking()
                        .Where(material => material.Id == detail.IdProduct.Value)
                        .Select(material => material.GramsPerMeter)
                        .FirstOrDefaultAsync();
                    if (conversion == null && gramsPerMeter is > 0)
                    {
                        detail.SourceMeasure = "m";
                        detail.GramsPerMeterAtMovement = gramsPerMeter;
                    }
                }

                await SaveDetailRowsWithTransferAsync(new List<Detailsinandout> { detail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        
        public async Task<Detailsinandout?> Update(int id, Detailsinandout detail)
        {
            var existingItem = await _context.Detailsinandout.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to update non-existent Detail with ID {Id}", id);
                return null;
            }
            var transferPairId = existingItem.IdTransferPair;

            try
            {
                detail.Id = id;
                await ApplyMaterialBrandAsync(detail, isNew: false);
                var movementType = await _context.Inandouts.AsNoTracking()
                    .Where(movement => movement.Id == detail.IdInandout)
                    .Select(movement => movement.Type)
                    .FirstOrDefaultAsync();
                if (movementType == "IN" && detail.IdProduct.HasValue)
                {
                    var material = await _context.Materials.AsNoTracking()
                        .Where(item => item.Id == detail.IdProduct.Value)
                        .Select(item => new { item.IdCompany, item.IdMedida })
                        .FirstOrDefaultAsync();
                    var sourceMeasureId = detail.SourceMeasureId ?? material?.IdMedida;
                    var configuredConversion = material != null && sourceMeasureId.HasValue
                        ? await _context.MaterialConversions.AsNoTracking()
                            .Where(item => item.IdCompany == material.IdCompany && item.IdMaterial == detail.IdProduct.Value
                                && item.IdMeasureFrom == sourceMeasureId.Value && item.Active)
                            .Select(item => new { item.Factor, item.IdMeasureTo })
                            .FirstOrDefaultAsync()
                        : null;
                    if (configuredConversion != null && detail.Quantity is > 0)
                    {
                        detail.SourceQuantity = detail.Quantity;
                        detail.SourceMeasure = await _context.Catalogs.AsNoTracking()
                            .Where(unit => unit.Id == sourceMeasureId.Value)
                            .Select(unit => unit.Description)
                            .FirstOrDefaultAsync();
                        detail.ConversionFactorAtMovement = configuredConversion.Factor;
                        detail.SourceMeasureId = sourceMeasureId;
                        detail.TargetMeasureAtMovement = await _context.Catalogs.AsNoTracking()
                            .Where(unit => unit.Id == configuredConversion.IdMeasureTo)
                            .Select(unit => unit.Description)
                            .FirstOrDefaultAsync();
                        detail.TargetMeasureIdAtMovement = configuredConversion.IdMeasureTo;
                        detail.Quantity *= configuredConversion.Factor;
                        _context.Entry(existingItem).CurrentValues.SetValues(detail);
                        existingItem.IdTransferPair = transferPairId;
                        await _context.SaveChangesAsync();
                        await SyncPairedDetailAsync(existingItem);
                        return existingItem;
                    }
                    var gramsPerMeter = await _context.Materials.AsNoTracking()
                        .Where(material => material.Id == detail.IdProduct.Value)
                        .Select(material => material.GramsPerMeter)
                        .FirstOrDefaultAsync();
                    if (gramsPerMeter is > 0 && detail.Quantity is > 0)
                    {
                        if (!IsKilogramUnit(detail.SourceMeasure))
                        {
                            var measureId = await _context.Materials.AsNoTracking()
                                .Where(material => material.Id == detail.IdProduct.Value)
                                .Select(material => material.IdMedida)
                                .FirstOrDefaultAsync();
                            var currentMeasure = measureId.HasValue
                                ? await _context.Catalogs.AsNoTracking()
                                    .Where(unit => unit.Id == measureId.Value)
                                    .Select(unit => unit.Description)
                                    .FirstOrDefaultAsync()
                                : null;
                            if (!IsKilogramUnit(currentMeasure))
                                throw new InvalidOperationException("El material con conversión gramos/metro debe tener unidad de compra en kilogramos.");
                            detail.SourceMeasure = currentMeasure;
                        }
                        detail.SourceQuantity = detail.Quantity;
                        detail.GramsPerMeterAtMovement = gramsPerMeter;
                        if (string.IsNullOrWhiteSpace(detail.SourceMeasure))
                        {
                            var measureId = await _context.Materials.AsNoTracking()
                                .Where(material => material.Id == detail.IdProduct.Value)
                                .Select(material => material.IdMedida)
                                .FirstOrDefaultAsync();
                            detail.SourceMeasure = measureId.HasValue
                                ? await _context.Catalogs.AsNoTracking()
                                    .Where(unit => unit.Id == measureId.Value)
                                    .Select(unit => unit.Description)
                                    .FirstOrDefaultAsync()
                                : "kg";
                        }
                        detail.Quantity = detail.Quantity.Value * 1000m / gramsPerMeter.Value;
                    }
                }
                else if (movementType == "OUT" && detail.IdProduct.HasValue && IsMeterUnit(detail.SourceMeasure))
                {
                    detail.GramsPerMeterAtMovement = await _context.Materials.AsNoTracking()
                        .Where(material => material.Id == detail.IdProduct.Value)
                        .Select(material => material.GramsPerMeter)
                        .FirstOrDefaultAsync();
                }
                if (movementType == "OUT") await ValidateBrandStockAsync(detail);
                _context.Entry(existingItem).CurrentValues.SetValues(detail);
                existingItem.IdTransferPair = transferPairId;
                await _context.SaveChangesAsync();
                await SyncPairedDetailAsync(existingItem);
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
            var existingItem = await _context.Detailsinandout.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to delete non-existent detail with ID {Id}", id);
                return false;
            }

            try
            {
                existingItem.Active = false;
                if (existingItem.IdTransferPair.HasValue)
                {
                    var pairedDetail = await _context.Detailsinandout.FindAsync(existingItem.IdTransferPair.Value);
                    if (pairedDetail != null) pairedDetail.Active = false;
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }

    public interface IDetailsinandoutService
    {
        Task<List<object>> GetDetails(int idInandout);
        Task<object> GetTotalInByProductAndWarehouse(int productId, int warehouseId, int? brandId = null);
        Task<object> GetStockByProductAndWarehouse(int productId, int warehouseId, int? brandId = null);
        Task Save(Detailsinandout detail);
        Task<Detailsinandout?> Update(int id, Detailsinandout detail);
        Task<bool> Delete(int id);
    }
