using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Service.Delison;

namespace Warehouse.Controllers.Delison
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly IExchangeRateService _service;
        private readonly ILogger<CurrencyController> _logger;

        public CurrencyController(IExchangeRateService service, ILogger<CurrencyController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // GET api/Currency/rate?moneda=USD&fecha=2026-06-08
        // Devuelve { tasa, fuente, fecha } o 204 (sin contenido) si ninguna fuente respondió (→ captura manual).
        [HttpGet("rate")]
        public async Task<IActionResult> GetRate([FromQuery] string moneda, [FromQuery] string? fecha = null)
        {
            try
            {
                DateOnly f = DateOnly.FromDateTime(DateTime.Now);
                if (!string.IsNullOrWhiteSpace(fecha) &&
                    DateTime.TryParse(fecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                    f = DateOnly.FromDateTime(dt);

                var r = await _service.GetRate(moneda ?? "", f);
                if (r == null) return NoContent();   // → el frontend pide el TC manual
                return Ok(r);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo tipo de cambio para {Moneda} {Fecha}", moneda, fecha);
                return StatusCode(500, "No se pudo obtener el tipo de cambio");
            }
        }
    }
}
