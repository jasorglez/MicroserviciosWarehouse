using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;

namespace Warehouse.Service.Delison
{
    public class ExchangeRateResult
    {
        public decimal Tasa { get; set; }
        public string Fuente { get; set; } = "";   // BANXICO | RESPALDO | MXN
        public string Fecha { get; set; } = "";     // yyyy-MM-dd efectiva del dato
    }

    public interface IExchangeRateService
    {
        /// <summary>Pesos por unidad de la moneda a la fecha dada. MXN → 1. null si ninguna fuente respondió.</summary>
        Task<ExchangeRateResult?> GetRate(string moneda, DateOnly fecha);
    }

    /// <summary>
    /// Fase 4: tipo de cambio a MXN. Orden: caché (currency_rates) → Banxico FIX (SIE) → API de
    /// respaldo (frankfurter) → null (el frontend pedirá captura manual). Cachea por (moneda, fecha).
    /// </summary>
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly DbWarehouseContext _context;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<ExchangeRateService> _logger;

        public ExchangeRateService(DbWarehouseContext context, IConfiguration config,
            IHttpClientFactory httpFactory, ILogger<ExchangeRateService> logger)
        {
            _context = context;
            _config = config;
            _httpFactory = httpFactory;
            _logger = logger;
        }

        public async Task<ExchangeRateResult?> GetRate(string moneda, DateOnly fecha)
        {
            var iso = (moneda ?? "").Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(iso) || iso == "MXN")
                return new ExchangeRateResult { Tasa = 1m, Fuente = "MXN", Fecha = fecha.ToString("yyyy-MM-dd") };

            // 1) Caché
            var cached = await _context.CurrencyRates.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Moneda == iso && r.Fecha == fecha);
            if (cached != null)
                return new ExchangeRateResult { Tasa = cached.Tasa, Fuente = cached.Fuente, Fecha = fecha.ToString("yyyy-MM-dd") };

            // 2) Banxico FIX
            try
            {
                var r = await FetchBanxico(iso, fecha);
                if (r != null) { await SaveCache(iso, fecha, r.Tasa, "BANXICO"); return r; }
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Banxico TC falló para {Iso} {Fecha}", iso, fecha); }

            // 3) Respaldo (frankfurter)
            try
            {
                var r = await FetchFallback(iso, fecha);
                if (r != null) { await SaveCache(iso, fecha, r.Tasa, "RESPALDO"); return r; }
            }
            catch (Exception ex) { _logger.LogWarning(ex, "TC respaldo falló para {Iso} {Fecha}", iso, fecha); }

            return null; // → captura manual en el frontend
        }

        private async Task SaveCache(string iso, DateOnly fecha, decimal tasa, string fuente)
        {
            try
            {
                _context.CurrencyRates.Add(new CurrencyRateDelison
                {
                    Moneda = iso, Fecha = fecha, Tasa = tasa, Fuente = fuente, DateCreated = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) { _logger.LogWarning(ex, "No se pudo cachear TC {Iso} {Fecha}", iso, fecha); }
        }

        private async Task<ExchangeRateResult?> FetchBanxico(string iso, DateOnly fecha)
        {
            var token = _config["ExchangeRate:BanxicoToken"];
            var baseUrl = _config["ExchangeRate:BanxicoBaseUrl"];
            var serie = _config[$"ExchangeRate:BanxicoSeries:{iso}"];
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(serie))
                return null;

            // Rango: 8 días atrás → fecha, para cubrir fines de semana/feriados (sin FIX publicado).
            var ini = fecha.AddDays(-8).ToString("yyyy-MM-dd");
            var fin = fecha.ToString("yyyy-MM-dd");
            var url = $"{baseUrl}/{serie}/datos/{ini}/{fin}?token={token}";

            var client = _httpFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(8);
            var json = await client.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);

            var datos = doc.RootElement.GetProperty("bmx").GetProperty("series")[0].GetProperty("datos");
            decimal? ultima = null; string fechaEfectiva = fin;
            foreach (var d in datos.EnumerateArray())
            {
                var datoStr = d.GetProperty("dato").GetString();
                if (string.IsNullOrWhiteSpace(datoStr) || datoStr == "N/E") continue;
                if (decimal.TryParse(datoStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) && val > 0)
                {
                    ultima = val;
                    var fStr = d.GetProperty("fecha").GetString();   // dd/MM/yyyy
                    if (DateTime.TryParseExact(fStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fdt))
                        fechaEfectiva = fdt.ToString("yyyy-MM-dd");
                }
            }
            if (ultima == null) return null;
            return new ExchangeRateResult { Tasa = ultima.Value, Fuente = "BANXICO", Fecha = fechaEfectiva };
        }

        private async Task<ExchangeRateResult?> FetchFallback(string iso, DateOnly fecha)
        {
            var baseUrl = _config["ExchangeRate:FallbackBaseUrl"];
            if (string.IsNullOrEmpty(baseUrl)) return null;
            // frankfurter: /{fecha}?from=ISO&to=MXN → { rates: { MXN: x }, date: "yyyy-MM-dd" }
            var url = $"{baseUrl}/{fecha:yyyy-MM-dd}?from={iso}&to=MXN";

            var client = _httpFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(8);
            var json = await client.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("rates", out var rates)) return null;
            if (!rates.TryGetProperty("MXN", out var mxn)) return null;
            var tasa = mxn.GetDecimal();
            if (tasa <= 0) return null;
            var fechaEfectiva = doc.RootElement.TryGetProperty("date", out var dEl) ? (dEl.GetString() ?? fecha.ToString("yyyy-MM-dd")) : fecha.ToString("yyyy-MM-dd");
            return new ExchangeRateResult { Tasa = tasa, Fuente = "RESPALDO", Fecha = fechaEfectiva };
        }
    }
}
