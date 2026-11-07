using System.Data;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse.Controllers;

[Authorize]
[ApiController]
[Route("api/Material/import")]
public sealed class MaterialImportController(DbWarehouseContext context, ILogger<MaterialImportController> logger) : ControllerBase
{
    public const int MaxRows = 5000;

    [HttpPost]
    [RequestSizeLimit(30_000_000)]
    public async Task<IActionResult> Import([FromBody] ImportRequest request, CancellationToken cancellationToken)
    {
        if (request.CompanyId <= 0 || request.Rows == null || request.Rows.Count is < 1 or > MaxRows)
            return BadRequest(new { message = $"Selecciona una empresa y envía entre 1 y {MaxRows} materiales." });
        var issues = ValidateRows(request.Rows);
        if (issues.Count > 0) return BadRequest(new { message = "Corrige las filas indicadas; no se importó ningún material.", errors = issues });
        try
        {
            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var catalogs = await context.Catalogs.AsNoTracking()
                .Where(c => c.IdCompany == request.CompanyId && c.Active == 1 &&
                    (c.Type.ToUpper() == "FAMILY" || c.Type.ToUpper() == "SUBFAMILY" || c.Type.ToUpper() == "MEASURE"))
                .ToListAsync(cancellationToken);
            var catalogIndex = catalogs.ToLookup(c => (Normalize(c.Type), Normalize(c.Description),
                Normalize(c.Type) == "SUBFAMILY" ? c.ParentId : null));
            var existing = await context.Materials.AsNoTracking().Where(m => m.IdCompany == request.CompanyId && m.Active == true)
                .Select(m => new { m.Insumo, m.Description }).ToListAsync(cancellationToken);
            var codes = existing.Select(m => Normalize(m.Insumo)).ToHashSet();
            var names = existing.Select(m => Normalize(m.Description)).ToHashSet();
            var materials = new List<Material>();
            foreach (var row in request.Rows)
            {
                if (codes.Contains(Normalize(row.Insumo))) issues.Add(new(row.ExcelRow, "El insumo ya existe en esta empresa."));
                if (names.Contains(Normalize(row.Description))) issues.Add(new(row.ExcelRow, "La descripción ya existe en esta empresa."));
                var family = Resolve(catalogIndex, "FAMILY", row.Family, null);
                var subfamily = family == null ? null : Resolve(catalogIndex, "SUBFAMILY", row.Subfamily, family.Id);
                var measure = Resolve(catalogIndex, "MEASURE", row.Measure, null);
                if (family == null) issues.Add(new(row.ExcelRow, "La familia no existe, está inactiva o su nombre es ambiguo en esta empresa."));
                if (subfamily == null) issues.Add(new(row.ExcelRow, "La subfamilia no pertenece a la familia indicada o no es válida en esta empresa."));
                if (measure == null) issues.Add(new(row.ExcelRow, "La unidad no existe, está inactiva o su nombre es ambiguo en esta empresa."));
                materials.Add(new Material {
                    IdCompany = request.CompanyId, Insumo = row.Insumo.Trim(), Description = row.Description.Trim(),
                    IdFamilia = family?.Id, IdSubfamilia = subfamily?.Id, IdMedida = measure?.Id,
                    BarCode = row.Barcode?.Trim() ?? "", CostoMN = row.Cost, VentaMN = row.Price,
                    StockMin = row.StockMin, StockMax = row.StockMax, GramsPerMeter = row.GramsPerMeter,
                    Quantity = 0, TypeMaterial = "CONSUMABLE", Active = true, Vigente = true, Date = DateTime.UtcNow.Date
                });
            }
            if (issues.Count > 0)
                return BadRequest(new { message = "Corrige las filas indicadas; no se importó ningún material.", errors = issues });
            context.Materials.AddRange(materials);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            logger.LogInformation("Importación de {Count} materiales para empresa {CompanyId} por {Actor}", materials.Count, request.CompanyId, User.Identity?.Name);
            return Ok(new { companyId = request.CompanyId, imported = materials.Count, message = "Los materiales se importaron correctamente." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error importando materiales para empresa {CompanyId}", request.CompanyId);
            return StatusCode(500, new { message = "No se completó la importación. La transacción se revirtió. Consulta el listado antes de volver a intentar." });
        }
    }

    private static Catalog? Resolve(ILookup<(string Type, string Name, int? ParentId), Catalog> catalogs, string type, string name, int? parentId)
    {
        var matches = catalogs[(type, Normalize(name), parentId)].Take(2).ToList();
        return matches.Count == 1 ? matches[0] : null;
    }

    public static List<ImportIssue> ValidateRows(List<ImportRow> rows)
    {
        var errors = new List<ImportIssue>();
        var codes = new HashSet<string>(); var names = new HashSet<string>();
        var numbers = new HashSet<int>();
        foreach (var row in rows)
        {
            if (row == null) { errors.Add(new(0, "El archivo contiene una fila nula.")); continue; }
            if (row.ExcelRow < 2 || !numbers.Add(row.ExcelRow)) errors.Add(new(row.ExcelRow, "Número de fila inválido o repetido."));
            if (string.IsNullOrWhiteSpace(row.Insumo) || row.Insumo.Trim().Length > 35) errors.Add(new(row.ExcelRow, "Insumo obligatorio, máximo 35 caracteres."));
            if (string.IsNullOrWhiteSpace(row.Description) || row.Description.Trim().Length > 1000) errors.Add(new(row.ExcelRow, "Descripción obligatoria, máximo 1000 caracteres."));
            if (string.IsNullOrWhiteSpace(row.Family) || string.IsNullOrWhiteSpace(row.Subfamily) || string.IsNullOrWhiteSpace(row.Measure)) errors.Add(new(row.ExcelRow, "Familia, subfamilia y unidad son obligatorias."));
            if ((row.Barcode?.Length ?? 0) > 50) errors.Add(new(row.ExcelRow, "Código de barras: máximo 50 caracteres."));
            if (row.Cost < 0 || row.Price < 0 || row.Cost > 99999999999999.99m || row.Price > 99999999999999.99m ||
                decimal.Round(row.Cost, 2) != row.Cost || decimal.Round(row.Price, 2) != row.Price)
                errors.Add(new(row.ExcelRow, "Precios no negativos, máximo dos decimales y 14 dígitos enteros."));
            if (row.StockMin < 0 || row.StockMax < row.StockMin) errors.Add(new(row.ExcelRow, "Stock mínimo y máximo enteros no negativos; máximo mayor o igual al mínimo."));
            if (row.GramsPerMeter.HasValue && (row.GramsPerMeter <= 0 || row.GramsPerMeter > 99999999999.9999m || decimal.Round(row.GramsPerMeter.Value, 4) != row.GramsPerMeter))
                errors.Add(new(row.ExcelRow, "Gramos por metro debe ser positivo, con máximo 4 decimales y 11 dígitos enteros."));
            if (!codes.Add(Normalize(row.Insumo))) errors.Add(new(row.ExcelRow, "Insumo repetido dentro del archivo."));
            if (!names.Add(Normalize(row.Description))) errors.Add(new(row.ExcelRow, "Descripción repetida dentro del archivo."));
        }
        return errors;
    }

    public static string Normalize(string? value) => string.Join(' ', string.Concat((value ?? "").Trim().Normalize(NormalizationForm.FormD)
        .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant();

    public sealed record ImportRequest(int CompanyId, List<ImportRow>? Rows);
    public sealed record ImportIssue(int Row, string Message);
    public sealed class ImportRow
    {
        public int ExcelRow { get; set; }
        public string Insumo { get; set; } = "";
        public string Description { get; set; } = "";
        public string Family { get; set; } = "";
        public string Subfamily { get; set; } = "";
        public string Measure { get; set; } = "";
        public string? Barcode { get; set; }
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
        public int StockMin { get; set; }
        public int StockMax { get; set; }
        public decimal? GramsPerMeter { get; set; }
    }
}
