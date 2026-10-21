using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Text;
using Warehouse.Models;
using Warehouse.Models.Views;


namespace Warehouse.Service
{
    public class MaterialService : IMaterialService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<MaterialService> _logger;
        private readonly string _connStr;

        public MaterialService(DbWarehouseContext context, ILogger<MaterialService> logger, IConfiguration config)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connStr = config.GetConnectionString("dbConWarehouse")!;
        }


        // MÉTODO MEJORADO usando la vista
        public async Task<List<MaterialsWithFamiliesView>> GetMaterialsWithFamilies(int idCompany)
        {
            try
            {
                var result = await _context.MaterialsWithFamiliesViews
                    .Where(m => m.IdCompany == idCompany && m.Active == true)
                    .OrderBy(m => m.FamiliaDescription)
                    .ThenBy(m => m.SubfamiliaDescription)
                    .ThenBy(m => m.MaterialDescription)
                    .AsNoTracking()
                    .ToListAsync();

                var materialIds = result.Select(item => item.Id).ToList();
                var conversionValues = await _context.Materials.AsNoTracking()
                    .Where(material => materialIds.Contains(material.Id))
                    .Select(material => new { material.Id, material.GramsPerMeter })
                    .ToDictionaryAsync(material => material.Id, material => material.GramsPerMeter);

                // Aplicar valores por defecto si es necesario
                foreach (var item in result)
                {
                    if (conversionValues.TryGetValue(item.Id, out var gramsPerMeter))
                        item.GramsPerMeter = gramsPerMeter;
                    item.Insumo = string.IsNullOrEmpty(item.Insumo) ? "N/A" : item.Insumo;
                    item.MedidaDescription = string.IsNullOrEmpty(item.MedidaDescription) ? "Unidad" : item.MedidaDescription;
                    item.MaterialDescription = string.IsNullOrEmpty(item.MaterialDescription) ? "Sin descripción" : item.MaterialDescription;
                    item.FamiliaDescription = string.IsNullOrEmpty(item.FamiliaDescription) ? "Sin familia" : item.FamiliaDescription;
                    item.SubfamiliaDescription = string.IsNullOrEmpty(item.SubfamiliaDescription) ? "Sin subfamilia" : item.SubfamiliaDescription;
                    item.Barcode = string.IsNullOrEmpty(item.Barcode) ? "N/A" : item.Barcode;
                    item.Picture = string.IsNullOrEmpty(item.Picture) ? "sin-imagen.jpg" : item.Picture;

                    item.CostoMN ??= 0;
                    item.VentaMN ??= 0;
                    item.StockMin ??= 0;
                    item.StockMax ??= 0;
                    item.Quantity ??= 0;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving materials with families for company {IdCompany}", idCompany);
                throw;
            }
        }

        public async Task<List<object>> GetMaterialBrands(int idCompany, int? idMaterial = null)
        {
            var query = from relation in _context.MaterialBrands.AsNoTracking()
                        join material in _context.Materials.AsNoTracking() on relation.IdMaterial equals material.Id
                        join catalog in _context.Catalogs.AsNoTracking() on relation.IdCatalog equals catalog.Id
                        where relation.Active && material.IdCompany == idCompany && material.Active == true
                            && catalog.Active == 1 && catalog.Type == "BRAND" && catalog.IdCompany == idCompany
                        select new { id = catalog.Id, relation.IdMaterial, brand = catalog.Description };

            if (idMaterial.HasValue)
                query = query.Where(item => item.IdMaterial == idMaterial.Value);

            return (await query.OrderBy(item => item.brand).ToListAsync())
                .Select(item => (object)item).ToList();
        }

        public async Task SaveMaterialBrands(int idCompany, int idMaterial, IEnumerable<int>? catalogIds)
        {
            var materialExists = await _context.Materials.AsNoTracking()
                .AnyAsync(item => item.Id == idMaterial && item.IdCompany == idCompany && item.Active == true);
            if (!materialExists) throw new InvalidOperationException("El material no existe o no pertenece a la empresa seleccionada.");

            var ids = (catalogIds ?? Enumerable.Empty<int>()).Where(id => id > 0).Distinct().ToList();
            var validIds = await _context.Catalogs.AsNoTracking()
                .Where(item => ids.Contains(item.Id) && item.IdCompany == idCompany && item.Type == "BRAND" && item.Active == 1)
                .Select(item => item.Id)
                .ToListAsync();
            if (validIds.Count != ids.Count)
                throw new InvalidOperationException("Una o más marcas no pertenecen al catálogo activo de la empresa.");

            var rows = await _context.MaterialBrands.Where(item => item.IdMaterial == idMaterial).ToListAsync();
            foreach (var row in rows) row.Active = ids.Contains(row.IdCatalog);
            foreach (var catalogId in ids)
            {
                var existing = rows.FirstOrDefault(item => item.IdCatalog == catalogId);
                if (existing == null)
                {
                    _context.MaterialBrands.Add(new MaterialBrand { IdMaterial = idMaterial, IdCatalog = catalogId, Active = true });
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<MaterialComponentResponse>> GetMaterialComponents(int idCompany, int idParentMaterial)
        {
            return await _context.MaterialComponents.AsNoTracking()
                .Where(component => component.IdCompany == idCompany && component.IdParentMaterial == idParentMaterial && component.Active)
                .OrderBy(component => component.Id)
                .Select(component => new MaterialComponentResponse
                {
                    ComponentName = component.ComponentName,
                    Quantity = component.Quantity,
                })
                .ToListAsync();
        }

        public async Task SaveMaterialComponents(int idCompany, int idParentMaterial, SaveMaterialComponentsRequest request)
        {
            if (request?.Components == null || idCompany <= 0 || idParentMaterial <= 0 || request.IdCompany != idCompany)
                throw new InvalidOperationException("La empresa o el material padre no son válidos.");

            var normalizedNames = request.Components.Select(component => component.ComponentName?.Trim() ?? string.Empty).ToList();
            if (request.Components.Any(component => string.IsNullOrWhiteSpace(component.ComponentName)
                    || component.ComponentName.Trim().Length > 200 || component.Quantity <= 0)
                || normalizedNames.Distinct(StringComparer.OrdinalIgnoreCase).Count() != normalizedNames.Count)
                throw new InvalidOperationException("Cada componente debe tener un nombre único y una cantidad mayor que cero.");

            var parent = await _context.Materials.AsNoTracking()
                .FirstOrDefaultAsync(material => material.Id == idParentMaterial && material.IdCompany == idCompany && material.Active == true);
            if (parent == null)
                throw new InvalidOperationException("El material padre no está activo en la empresa seleccionada.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            var previous = await _context.MaterialComponents
                .Where(component => component.IdCompany == idCompany && component.IdParentMaterial == idParentMaterial && component.Active)
                .ToListAsync();
            foreach (var component in previous) component.Active = false;

            await _context.SaveChangesAsync();

            _context.MaterialComponents.AddRange(request.Components.Select(component => new MaterialComponent
            {
                IdCompany = idCompany,
                IdParentMaterial = idParentMaterial,
                ComponentName = component.ComponentName.Trim(),
                Quantity = component.Quantity,
                Active = true,
            }));
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }


        public async Task<List<MaterialConversionResponse>> GetMaterialConversions(int idCompany, int? idMaterial = null)
        {
            var query = from conversion in _context.MaterialConversions.AsNoTracking()
                        join measureFrom in _context.Catalogs.AsNoTracking() on conversion.IdMeasureFrom equals measureFrom.Id
                        join measureTo in _context.Catalogs.AsNoTracking() on conversion.IdMeasureTo equals measureTo.Id
                        where conversion.IdCompany == idCompany && conversion.Active
                            && (!idMaterial.HasValue || conversion.IdMaterial == idMaterial.Value)
                        orderby conversion.Id
                        select new MaterialConversionResponse
                        {
                            IdMaterial = conversion.IdMaterial,
                            IdMeasureFrom = conversion.IdMeasureFrom,
                            MeasureFrom = measureFrom.Description,
                            IdMeasureTo = conversion.IdMeasureTo,
                            MeasureTo = measureTo.Description,
                            Factor = conversion.Factor,
                        };
            return await query.ToListAsync();
        }

        public async Task SaveMaterialConversions(int idCompany, int idMaterial, SaveMaterialConversionsRequest request)
        {
            if (request?.Conversions == null || idCompany <= 0 || idMaterial <= 0 || request.IdCompany != idCompany)
                throw new InvalidOperationException("La empresa o el material no son válidos.");

            if (request.Conversions.Any(conversion => conversion.IdMeasureFrom <= 0 || conversion.IdMeasureTo <= 0
                    || conversion.IdMeasureFrom == conversion.IdMeasureTo || conversion.Factor <= 0)
                || request.Conversions.Select(conversion => conversion.IdMeasureFrom).Distinct().Count() != request.Conversions.Count
                || request.Conversions.Select(conversion => conversion.IdMeasureTo).Distinct().Count() > 1)
                throw new InvalidOperationException("Cada unidad origen debe ser única, usar un factor mayor que cero y convertir a la misma unidad de inventario.");

            var parent = await _context.Materials.AsNoTracking()
                .FirstOrDefaultAsync(material => material.Id == idMaterial && material.IdCompany == idCompany && material.Active == true);
            if (parent == null)
                throw new InvalidOperationException("El material no está activo en la empresa seleccionada.");

            if (request.Conversions.Count > 0 && (!parent.IdMedida.HasValue
                || !request.Conversions.Any(conversion => conversion.IdMeasureFrom == parent.IdMedida.Value)))
                throw new InvalidOperationException("Incluye la unidad de compra del material como una de las unidades origen.");

            var measureIds = request.Conversions.SelectMany(conversion => new[] { conversion.IdMeasureFrom, conversion.IdMeasureTo }).Distinct().ToList();
            var validMeasureIds = await _context.Catalogs.AsNoTracking()
                .Where(measure => measureIds.Contains(measure.Id) && measure.IdCompany == idCompany
                    && measure.Type == "MEASURE" && measure.Active == 1)
                .Select(measure => measure.Id)
                .ToListAsync();
            if (validMeasureIds.Count != measureIds.Count)
                throw new InvalidOperationException("Selecciona unidades de medida activas de esta empresa.");

            var currentConversions = await _context.MaterialConversions.AsNoTracking()
                .Where(conversion => conversion.IdCompany == idCompany && conversion.IdMaterial == idMaterial && conversion.Active)
                .ToListAsync();
            var hasInventoryHistory = await _context.Detailsinandout.AsNoTracking()
                .Join(_context.Inandouts.AsNoTracking(), detail => detail.IdInandout, movement => movement.Id,
                    (detail, movement) => new { detail, movement })
                .AnyAsync(row => row.detail.Active && row.detail.IdProduct == idMaterial && row.movement.Active == true && row.movement.Type == "IN");
            var newTargetMeasureId = request.Conversions.FirstOrDefault()?.IdMeasureTo;
            var currentTargetMeasureId = currentConversions.FirstOrDefault()?.IdMeasureTo;
            if (hasInventoryHistory && ((currentConversions.Count == 0 && request.Conversions.Count > 0)
                || (currentConversions.Count > 0 && (request.Conversions.Count == 0 || currentTargetMeasureId != newTargetMeasureId))))
                throw new InvalidOperationException("Este material ya tiene entradas de inventario. No se puede cambiar la unidad base; configura la conversión antes de registrar entradas.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            foreach (var conversion in currentConversions) conversion.Active = false;
            await _context.SaveChangesAsync();

            _context.MaterialConversions.AddRange(request.Conversions.Select(conversion => new MaterialConversion
            {
                IdCompany = idCompany,
                IdMaterial = idMaterial,
                IdMeasureFrom = conversion.IdMeasureFrom,
                IdMeasureTo = conversion.IdMeasureTo,
                Factor = conversion.Factor,
                Active = true,
            }));
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        public async Task<List<ProveedoresxtypeView>> MaterialsxProvView(int idCompany)
        {
            try
            {
                var result = await _context.ProveedoresxtypeViews
                    .Where(m => m.IdRoot == idCompany)
                    .OrderByDescending(m => m.Vigente)
                    .ThenBy(m => string.IsNullOrEmpty(m.Company) ? 1 : 0) // Con company primero (0), sin company después (1)
                    .ThenBy(m => string.IsNullOrEmpty(m.Company) ? m.NameContact : m.Company)
                    .ThenBy(m => m.NameContact) // Orden adicional por namecontact como desempate
                    .AsNoTracking()
                    .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Materials view");
                throw;
            }
        }


        public async Task<List<MaterialWithCount>> GetMaterialsWithCounts(int idCompany)
        {
            try
            {
                var result = await _context.MaterialWithCounts
                    .Where(m => m.IdCompany == idCompany)
                    .OrderByDescending(m => m.Vigente)
                    .ThenBy(m => m.Articulo)
                    .AsNoTracking()
                    .ToListAsync();

                // Aplicar valores por defecto después de cargar desde BD
                foreach (var item in result)
                {
                    item.Insumo     = string.IsNullOrEmpty(item.Insumo) ? "N/A" : item.Insumo;
                    item.Articulo   = string.IsNullOrEmpty(item.Articulo) ? "Sin especificar" : item.Articulo;
                    item.Categoria  = string.IsNullOrEmpty(item.Categoria) ? "Sin categoría" : item.Categoria;
                    item.Familia    = string.IsNullOrEmpty(item.Familia) ? "Sin familia" : item.Familia;
                    item.Subfamilia = string.IsNullOrEmpty(item.Subfamilia) ? "Sin subfamilia" : item.Subfamilia;
                    item.Picture    = string.IsNullOrEmpty(item.Picture) ? "sin-imagen.jpg" : item.Picture;
                    item.Merma      ??= 0m;
                    item.Fecha      ??= DateTime.MinValue;
                    item.ProviderCount ??= 0;
                    item.Parametros ??= 0;
                    item.SubfamilyCount ??= 0;
                    item.Costo ??= 0m;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Materials with counts for company {IdCompany}", idCompany);
                throw;
            }
        }

        public async Task<List<MaterialsXProvider>> MaterialsxProv(string Typereference, int idProv)
        {
            try
            {
                var result = await _context.MaterialsxProviders
                    .Where(m => m.IdProvider==idProv)
                    .OrderBy(m => m.TypeReference==Typereference)
                    .AsNoTracking()
                    .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Materials view");
                throw;
            }
        }


        public async Task<List<MaterialsxProvExist>> GetMaterialsView(int idCompany)
        {
            try
            {
                var result = await _context.MaterialsxProvExists
                    .Where(m => m.IdCompany == idCompany)
                    .OrderBy(m => m.Description)
                    .AsNoTracking()
                    .ToListAsync();

                // Aplicar valores por defecto después de cargar desde BD
                foreach (var item in result)
                {
                    // Strings - aplicar valores por defecto si son null o vacíos
                    item.TypeOcorReq = string.IsNullOrEmpty(item.TypeOcorReq) ? "N/A" : item.TypeOcorReq;
                    item.Insumo = string.IsNullOrEmpty(item.Insumo) ? "N/A" : item.Insumo;
                    item.Barcode = string.IsNullOrEmpty(item.Barcode) ? "N/A" : item.Barcode;
                    item.Company = string.IsNullOrEmpty(item.Company) ? "N/A" : item.Company;
                    item.Articulo = string.IsNullOrEmpty(item.Articulo) ? "Sin especificar" : item.Articulo;
                    item.Description = string.IsNullOrEmpty(item.Description) ? "Sin descripción" : item.Description;
                    item.Folio = string.IsNullOrEmpty(item.Folio) ? "N/A" : item.Folio;
                    item.Picture = string.IsNullOrEmpty(item.Picture) ? "sin-imagen.jpg" : item.Picture;
                    item.DescriptionPackage = string.IsNullOrEmpty(item.DescriptionPackage) ? "N/A" : item.DescriptionPackage;
                    item.Measure = string.IsNullOrEmpty(item.Measure) ? "Unidad" : item.Measure;
                    item.FolioOcorReq = string.IsNullOrEmpty(item.FolioOcorReq) ? "N/A" : item.FolioOcorReq;

                    // Decimales - asignar 0 si son null
                    item.Price ??= 0;
                    item.Quantity ??= 0;
                    item.CostoMN ??= 0;
                    item.CostoDLL ??= 0;
                    item.VentaMN ??= 0;
                    item.VentaDLL ??= 0;
                    item.PackageQuantity ??= 1;
                    item.WeightOrVolumes ??= 0;
                    item.InOrOutQuantity ??= 0;
                    item.Pending ??= 0;

                    // Enteros - asignar 0 si son null
                    item.IdBranch ??= 0;
                    item.IdCustomer ??= 0;
                    item.IdCategory ??= 0;
                    item.IdFamilia ??= 0;
                    item.IdSubfamilia ??= 0;
                    item.IdMedida ??= 0;
                    item.IdUbication ??= 0;
                    item.StockMin ??= 0;
                    item.StockMax ??= 0;
                    item.Expiration ??= 0;

                    // Booleans - asignar false si son null
                    item.AplicaResg ??= false;
                    item.Vigente ??= true; // Este probablemente debería ser true por defecto

                    // DateTime - puedes asignar fecha actual o una fecha por defecto
                    // item.Date ??= DateTime.Now; // Solo si lo necesitas
                    // item.FechaOc ??= DateTime.Now; // Solo si lo necesitas
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Materials view");
                throw;
            }
        }

        public async Task<List<object>> GetSupplies(int idCompany, string typematerial)
        {
            try
            {
                return await _context.Materials
                    .Where(s => s.Active == true && s.IdCompany == idCompany && s.TypeMaterial == typematerial)         
                    .Select(s => new
                    {
                        s.Id,                        
                        s.IdCompany,
                        s.IdBranch,
                        s.IdCustomer,
                        s.Insumo,
                        s.BarCode,
                        s.Articulo,
                        s.IdCategory,
                        s.IdFamilia,
                        s.IdSubfamilia,
                        s.IdMedida,
                        s.IdUbication,
                        s.Description,
                        s.Date,
                        s.AplicaResg,
                        s.CostoMN,
                        s.CostoDLL,
                        s.VentaMN,
                        s.VentaDLL,
                        s.StockMin, s.StockMax, s.Picture,
                        s.Vigente,  s.TypeMaterial,
 

                        s.Active
                       
                    })
                    .OrderByDescending(s => s.Vigente)
                    .ThenBy(s => s.Description)
                    .AsNoTracking()
                    .ToListAsync<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Supplies");
                throw;
            }
        }

        public async Task<List<object>> Get2Supplies(int idCompany)
        {
            try
            {
                return await _context.Materials
                    .Where(s => s.Active == true && idCompany == s.IdCompany)
                    .Select(s => new
                    {
                        s.Id,
                        s.Description,
                        code = s.Insumo,
                        // La OC usa este catálogo reducido para buscar el
                        // material; también necesita el precio de venta para
                        // precargar la partida seleccionada.
                        s.VentaMN,
                        idMeasure = s.IdMedida,
                        measure = _context.Catalogs
                            .Where(c => c.Id == s.IdMedida && c.Type == "MEASURE" && c.Active == 1)
                            .Select(c => c.Description)
                            .FirstOrDefault() ?? "",
                        s.Vigente,
                        s.GramsPerMeter,
                        s.Active,
                    })
                    .OrderByDescending(s => s.Vigente)
                    .ThenBy(s => s.Description)
                    .AsNoTracking()
                    .ToListAsync<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Supplies");
                throw;
            }
        }

        public async Task<List<object>> GetMaterialsForApu(int idCompany)
        {
            try
            {
                var conn = _context.Database.GetDbConnection();
                if (conn.State != System.Data.ConnectionState.Open)
                    await conn.OpenAsync();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $@"
                    SELECT m.id,
                           m.description,
                           ISNULL(ms.description, '')        AS measure,
                           ISNULL(m.costoMN, 0)              AS costoMN,
                           ISNULL(m.ventaMN, 0)               AS ventaMN,
                           ISNULL(cf.description, '')        AS familia
                    FROM   materials m
                    LEFT JOIN catalog ms ON ms.id = m.id_medida AND ms.type = 'MEASURE'
                    LEFT JOIN catalog cf ON cf.id = m.id_familia AND cf.type = 'FAMILY'
                    WHERE  m.id_company = {idCompany}
                      AND  m.vigente = 1
                      AND  m.active  = 1
                    ORDER  BY articulo";

                using var reader = await cmd.ExecuteReaderAsync();
                var result = new List<object>();
                while (await reader.ReadAsync())
                {
                    result.Add(new
                    {
                        id          = reader.GetInt32(0),
                        description = reader.GetString(1),
                        measure     = reader.GetString(2),
                        costoMN     = reader.GetDecimal(3),
                        ventaMN     = reader.GetDecimal(4),
                        familia     = reader.GetString(5),
                    });
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving materials for APU company {IdCompany}", idCompany);
                throw;
            }
        }

        public async Task<List<object>> GetSuppliesByNameOrBarCode(int idCompany, string nameOrBarCode)
        {
            try
            {
                return await _context.Materials
                    .Where(s => s.Active == true && idCompany == s.IdCompany &&
                    (s.Insumo.Contains(nameOrBarCode) || s.BarCode.Contains(nameOrBarCode) || s.Description.Contains(nameOrBarCode)))
                    //   .Include(s => s.PricesWithMaterial)
                    .Select(s => new
                    {
                        s.Id,
                        s.BarCode,
                        s.Insumo,
                        s.Description,
                        s.Active,
                        price = s.VentaMN ?? 0m,
                    }).OrderBy(s => s.Description)
                    .AsNoTracking()
                    .ToListAsync<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Supplies");
                throw;
            }
        }

        public async Task<bool> CatalogByMaterial(int idCatalog)
        {
            try
            {
                var exists = await _context.Materials
                    .AnyAsync(s => s.Active == true && 
                        (s.IdCategory == idCatalog || 
                         s.IdFamilia == idCatalog || 
                         s.IdSubfamilia == idCatalog));

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Supplies");
                throw;
            }
        }


        public async Task Save(Material material)
        {
            try
            {
                var normalizedDescription = NormalizeDescription(material.Description);
                if (string.IsNullOrWhiteSpace(normalizedDescription))
                    throw new InvalidOperationException("La descripción del material es obligatoria.");

                // Protege altas simultáneas o realizadas fuera de la ventana
                // de OC. No distingue mayúsculas, acentos ni espacios extra.
                var materialDescriptions = await _context.Materials
                    .Where(m => m.Active == true && m.IdCompany == material.IdCompany)
                    .Select(m => m.Description)
                    .ToListAsync();
                if (materialDescriptions.Any(description =>
                    NormalizeDescription(description) == normalizedDescription))
                {
                    throw new InvalidOperationException($"Ya existe un material con el nombre '{material.Description?.Trim()}'.");
                }

               var materials = await _context.Materials
                    .Where(m => m.Active == true &&
                                m.IdCompany == material.IdCompany &&
                                m.IdSubfamilia == material.IdSubfamilia)
                    .ToListAsync();

                _logger.LogInformation("Materiales encontrados para IdCompany {IdCompany} y IdSubfamilia {IdSubfamilia}: {Count}",
                    material.IdCompany, material.IdSubfamilia, materials.Count);
                var CalculadorIsumo = "";
                if(materials.Count == 0)
                {
                    var cat = await _context.Catalogs
                        .Where(c => c.Id == material.IdCategory)
                        .FirstOrDefaultAsync();
                    var fam = await _context.Catalogs
                        .Where(c => c.Id == material.IdFamilia)
                        .FirstOrDefaultAsync();
                    var subfam = await _context.Catalogs
                        .Where(c => c.Id == material.IdSubfamilia)
                        .FirstOrDefaultAsync();
                    var data = cat?.ValueAddition2 + fam?.ValueAddition2 + subfam?.ValueAddition2 + "-0001";
                    CalculadorIsumo = data;
                }
                else
                {
                    // Si quieres imprimir cada material individualmente
                    var ultimo = materials.LastOrDefault();
                        _logger.LogInformation("Material -> Id: {Id}, Articulo: {Articulo}, Insumo: {Insumo}",
                            ultimo.Id, ultimo.Articulo, ultimo.Insumo);
                    var cat = await _context.Catalogs
                        .Where(c => c.Id == ultimo.IdCategory)
                        .FirstOrDefaultAsync();
                    var fam = await _context.Catalogs
                        .Where(c => c.Id == ultimo.IdFamilia)
                        .FirstOrDefaultAsync();
                    var subfam = await _context.Catalogs
                        .Where(c => c.Id == ultimo.IdSubfamilia)
                        .FirstOrDefaultAsync();
                    _logger.LogInformation("Categoría: {Categoria}, Familia: {Familia}, Subfamilia: {Subfamilia}",
                        cat?.ValueAddition2, fam?.ValueAddition2, subfam?.ValueAddition2);
                    var partes = ultimo.Insumo.Split('-');
                    var contador = "0001";

                    if (partes.Length == 2)
                    {
                        var numeroStr = partes[1];     // "0002"

                        if (int.TryParse(numeroStr, out int numero))
                        {
                            numero++; // sumar 1

                            // Mantener mismo formato de ceros a la izquierda
                            var nuevoNumero = numero.ToString(new string('0', numeroStr.Length)); 

                            var nuevoArticulo = $"{partes[0]}-{nuevoNumero}";
                            CalculadorIsumo = nuevoArticulo;

                            _logger.LogInformation("Nuevo Articulo generado: {Nuevo}", nuevoArticulo);
                        }
                    }
                    _logger.LogInformation("Nuevo Insumo generado: {NuevoInsumo}", CalculadorIsumo);
                }

                // Si el usuario envió un Insumo propio, usarlo. Si no, usar el generado automáticamente.
                // Fallback final: timestamp si el auto-generador quedó vacío.
                var insumoFinal = !string.IsNullOrWhiteSpace(material.Insumo)
                    ? material.Insumo
                    : !string.IsNullOrWhiteSpace(CalculadorIsumo)
                        ? CalculadorIsumo
                        : $"MAT-{DateTime.UtcNow:yyyyMMddHHmmss}";

                _logger.LogInformation("Insumo final a guardar: {InsumoFinal} (usuario={InsumoUsuario}, calculado={CalculadorIsumo})",
                    insumoFinal, material.Insumo, CalculadorIsumo);

              // Crear una nueva instancia con los campos necesarios
                var newMaterial = new Material
                {
                    // Asignar campos del material recibido
                    IdCompany = material.IdCompany,
                    IdBranch = material.IdBranch,
                    IdCustomer = material.IdCustomer,
                    Insumo = insumoFinal,
                    Articulo = material.Articulo,
                    BarCode = material.BarCode,
                    IdCategory = material.IdCategory,
                    IdFamilia = material.IdFamilia,
                    IdSubfamilia = material.IdSubfamilia,
                    IdMedida = material.IdMedida,
                    IdUbication = material.IdUbication,
                    Description = material.Description,
                    Date = material.Date ?? DateTime.UtcNow, // Valor por defecto si es null
                    AplicaResg = material.AplicaResg,
                    CostoMN = material.CostoMN,
                    CostoDLL = material.CostoDLL,
                    VentaMN = material.VentaMN,
                    VentaDLL = material.VentaDLL,
                    StockMin = material.StockMin,
                    StockMax = material.StockMax,
                    Quantity = material.Quantity ?? 0,
                    RequestQuantity = material.RequestQuantity ?? 0,
                    Picture = material.Picture,
                    TypeMaterial = material.TypeMaterial,
                    Merma = material.Merma,
                    Fecha = material.Date ?? DateTime.UtcNow,
                    Vigente = material.Vigente ?? true,
                    Active = material.Active ?? true
                };

                _context.Materials.Add(newMaterial);
                await _context.SaveChangesAsync();

                // El controlador responde el objeto recibido. Propagar el ID
                // generado evita que el cliente interprete el alta como fallida
                // aunque el material sí haya quedado guardado.
                material.Id = newMaterial.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Material");
                throw;
            }
        }

        private static string NormalizeDescription(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            var withoutAccents = string.Concat(value.Trim()
                .Normalize(NormalizationForm.FormD)
                .Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark));

            return string.Join(' ', withoutAccents
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .ToUpperInvariant();
        }

        public async Task<Material?> Update(int id, Material material)
        {
            var existingItem = await _context.Materials.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to update non-existent Supply with ID {Id}", id);
                return null;
            }

            try
            {
                // Solo actualizar campos que vienen con valor (partial update / merge)
                // Esto permite ediciones parciales sin perder datos existentes
                if (material.IdCompany.HasValue) existingItem.IdCompany = material.IdCompany;
                if (material.IdBranch.HasValue) existingItem.IdBranch = material.IdBranch;
                if (material.IdCustomer.HasValue) existingItem.IdCustomer = material.IdCustomer;
                if (!string.IsNullOrEmpty(material.Insumo)) existingItem.Insumo = material.Insumo;
                if (material.Fecha.HasValue) existingItem.Fecha = material.Fecha;
                if (material.Merma.HasValue) existingItem.Merma = material.Merma;
                if (!string.IsNullOrEmpty(material.Articulo)) existingItem.Articulo = material.Articulo;
                if (!string.IsNullOrEmpty(material.BarCode)) existingItem.BarCode = material.BarCode;
                if (material.IdCategory.HasValue) existingItem.IdCategory = material.IdCategory;
                if (material.IdFamilia.HasValue) existingItem.IdFamilia = material.IdFamilia;
                if (material.IdSubfamilia.HasValue) existingItem.IdSubfamilia = material.IdSubfamilia;
                if (material.IdMedida.HasValue) existingItem.IdMedida = material.IdMedida;
                if (material.IdUbication.HasValue) existingItem.IdUbication = material.IdUbication;
                if (!string.IsNullOrEmpty(material.Description)) existingItem.Description = material.Description;
                if (material.Date.HasValue) existingItem.Date = material.Date;
                if (material.AplicaResg.HasValue) existingItem.AplicaResg = material.AplicaResg;
                if (material.CostoMN.HasValue) existingItem.CostoMN = material.CostoMN;
                if (material.CostoDLL.HasValue) existingItem.CostoDLL = material.CostoDLL;
                if (material.VentaMN.HasValue) existingItem.VentaMN = material.VentaMN;
                if (material.VentaDLL.HasValue) existingItem.VentaDLL = material.VentaDLL;
                if (material.StockMin.HasValue) existingItem.StockMin = material.StockMin;
                if (material.StockMax.HasValue) existingItem.StockMax = material.StockMax;
                existingItem.GramsPerMeter = material.GramsPerMeter;
                existingItem.Quantity = material.Quantity ?? 0;
                existingItem.RequestQuantity = material.RequestQuantity ?? 0;
                if (!string.IsNullOrEmpty(material.Picture)) existingItem.Picture = material.Picture;
                if (!string.IsNullOrEmpty(material.TypeMaterial)) existingItem.TypeMaterial = material.TypeMaterial;
                if (material.Vigente.HasValue) existingItem.Vigente = material.Vigente;
                if (material.Active.HasValue) existingItem.Active = material.Active;

                await _context.SaveChangesAsync();
                return existingItem;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating Supply with ID {Id}", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Supply with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            var existingItem = await _context.Materials.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to delete non-existent Supply with ID {Id}", id);
                return false;
            }

            // Validar que el producto no haya sido vendido en ningún POS
            try
            {
                await using var conn = new SqlConnection(_connStr);
                await conn.OpenAsync();
                await using var cmd = new SqlCommand(
                    "SELECT TOP 1 1 FROM administration.pv.salesxconcept WHERE id_product = @id AND active = 1",
                    conn);
                cmd.Parameters.AddWithValue("@id", id);
                var sold = await cmd.ExecuteScalarAsync();
                if (sold != null)
                    throw new InvalidOperationException("TIENE_VENTAS");
            }
            catch (InvalidOperationException) { throw; }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo verificar ventas para material {Id} — se permite el borrado", id);
                // Si la consulta cross-DB falla (permisos, etc.), no bloqueamos el borrado
            }

            try
            {
                existingItem.Active = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Supply with ID {Id}", id);
                throw;
            }
        }

        public async Task<List<MaterialsByProviderView>> GetMaterialsByProvider(int idProvider)
        {
            try
            {
                var result = await _context.MaterialsByProviderViews
                    .Where(m => m.IdProvider == idProvider)
                    .OrderBy(m => m.Nombre)
                    .AsNoTracking()
                    .ToListAsync();

                // Aplicar valores por defecto después de cargar desde BD
                foreach (var item in result)
                {
                    item.Codigo = string.IsNullOrEmpty(item.Codigo) ? "N/A" : item.Codigo;
                    item.Nombre = string.IsNullOrEmpty(item.Nombre) ? "Sin especificar" : item.Nombre;
                    item.Categoria = string.IsNullOrEmpty(item.Categoria) ? "Sin categoría" : item.Categoria;
                    item.Familia = string.IsNullOrEmpty(item.Familia) ? "Sin familia" : item.Familia;
                    item.Subfamilia = string.IsNullOrEmpty(item.Subfamilia) ? "Sin subfamilia" : item.Subfamilia;
                    item.Precio ??= 0;
                    item.Vigente ??= true;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving materials by provider {IdProvider}", idProvider);
                throw;
            }
        }

        public async Task<List<VwListProvidersForRawMaterial>> GetProvidersByMaterialAndType(int idMaterial, string? typeIntOrExt)
        {
            try
            {
                var query = _context.VwListProvidersForRawMaterials
                    .Where(p => p.IdMaterial == idMaterial);

                if (!string.IsNullOrEmpty(typeIntOrExt))
                {
                    query = query.Where(p => p.TypeIntOrExt == typeIntOrExt);
                }

                var providers = await query
                    .OrderBy(p => p.ProviderName)
                    .AsNoTracking()
                    .ToListAsync();

                return providers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving providers for material {IdMaterial} and type {TypeIntOrExt}", idMaterial, typeIntOrExt);
                throw;
            }
        }
    }

    public interface IMaterialService
    {
        Task<List<MaterialsWithFamiliesView>> GetMaterialsWithFamilies(int idCompany);
        Task<List<object>> GetMaterialBrands(int idCompany, int? idMaterial = null);
        Task SaveMaterialBrands(int idCompany, int idMaterial, IEnumerable<int>? catalogIds);
        Task<List<MaterialComponentResponse>> GetMaterialComponents(int idCompany, int idParentMaterial);
        Task SaveMaterialComponents(int idCompany, int idParentMaterial, SaveMaterialComponentsRequest request);
        Task<List<MaterialConversionResponse>> GetMaterialConversions(int idCompany, int? idMaterial = null);
        Task SaveMaterialConversions(int idCompany, int idMaterial, SaveMaterialConversionsRequest request);
        Task<List<ProveedoresxtypeView>> MaterialsxProvView(int idCompany);
        Task<List<MaterialWithCount>> GetMaterialsWithCounts(int idCompany);
        Task<List<MaterialsXProvider>> MaterialsxProv(string Typereference, int idProv);
        Task<List<object>> GetSupplies(int idCompany, string typematerial);
        Task<bool> CatalogByMaterial(int idCatalog);
        Task<List<object>> Get2Supplies(int idCompany);
        Task<List<object>> GetMaterialsForApu(int idCompany);
        Task<List<MaterialsxProvExist>> GetMaterialsView(int idCompany);
        Task<List<object>> GetSuppliesByNameOrBarCode(int idCompany, string nameOrBarCode);
        Task<List<MaterialsByProviderView>> GetMaterialsByProvider(int idProvider);
        Task<List<VwListProvidersForRawMaterial>> GetProvidersByMaterialAndType(int idMaterial, string? typeIntOrExt);
        Task Save(Material material);
        Task<Material?> Update(int id, Material material);
        Task<bool> Delete(int id);
    }

}

    
