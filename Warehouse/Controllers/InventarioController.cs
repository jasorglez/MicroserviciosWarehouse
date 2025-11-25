using Microsoft.AspNetCore.Mvc;
using Warehouse.Service;
using Warehouse.Models.Views;

namespace Warehouse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventarioController : ControllerBase
    {
        private readonly IInventarioService _inventarioService;
        private readonly ILogger<InventarioController> _logger;

        public InventarioController(IInventarioService inventarioService, ILogger<InventarioController> logger)
        {
            _inventarioService = inventarioService ?? throw new ArgumentNullException(nameof(inventarioService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtiene todo el inventario total ordenado por insumo
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<VwInventarioTotal>>> GetInventarioTotal()
        {
            try
            {
                _logger.LogInformation("Obteniendo inventario total");
                var inventario = await _inventarioService.ObtenerInventarioTotalAsync();
                return Ok(inventario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el inventario total");
                return StatusCode(500, new { error = "Error interno del servidor al obtener el inventario" });
            }
        }

        /// <summary>
        /// Obtiene el inventario filtrado por compañía
        /// </summary>
        /// <param name="idCompany">ID de la compañía (default: 28)</param>
        [HttpGet("filtrado/{idCompany?}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<VwInventarioTotal>>> GetInventarioFiltrado(int idCompany)
        {
            try
            {
                if (idCompany <= 0)
                {
                    _logger.LogWarning("ID de compañía inválido: {IdCompany}", idCompany);
                    return BadRequest(new { error = "El ID de la compañía debe ser mayor a 0" });
                }

                _logger.LogInformation("Obteniendo inventario filtrado para compañía {IdCompany}", idCompany);
                var inventario = await _inventarioService.ObtenerInventarioFiltradoAsync(idCompany);

                if (!inventario.Any())
                {
                    _logger.LogInformation("No se encontraron registros para la compañía {IdCompany}", idCompany);
                    return Ok(Enumerable.Empty<VwInventarioTotal>());
                }

                return Ok(inventario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el inventario filtrado para compañía {IdCompany}", idCompany);
                return StatusCode(500, new { error = "Error interno del servidor al obtener el inventario filtrado" });
            }
        }

        /// <summary>
        /// Obtiene un resumen general del inventario
        /// </summary>
        [HttpGet("resumen")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> GetResumenInventario()
        {
            try
            {
                _logger.LogInformation("Obteniendo resumen del inventario");
                var resumen = await _inventarioService.ObtenerResumenAsync();
                return Ok(resumen);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el resumen del inventario");
                return StatusCode(500, new { error = "Error interno del servidor al obtener el resumen" });
            }
        }

        /// <summary>
        /// Obtiene productos con stock crítico
        /// </summary>
        [HttpGet("criticos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<VwInventarioTotal>>> GetProductosCriticos()
        {
            try
            {
                _logger.LogInformation("Obteniendo productos con stock crítico");
                var inventario = await _inventarioService.ObtenerInventarioTotalAsync();
                var criticos = inventario.Where(v => v.EstadoStock == "CRÍTICO");
                return Ok(criticos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos críticos");
                return StatusCode(500, new { error = "Error interno del servidor al obtener productos críticos" });
            }
        }

        /// <summary>
        /// Busca productos por término en insumo o descripción
        /// </summary>
        /// <param name="termino">Término de búsqueda</param>
        [HttpGet("buscar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<VwInventarioTotal>>> BuscarProductos([FromQuery] string termino)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino))
                {
                    _logger.LogWarning("Término de búsqueda vacío");
                    return BadRequest(new { error = "El término de búsqueda no puede estar vacío" });
                }

                _logger.LogInformation("Buscando productos con término: {Termino}", termino);
                var inventario = await _inventarioService.ObtenerInventarioTotalAsync();

                var resultados = inventario
                    .Where(v => v.Insumo.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                               v.Description.Contains(termino, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(v => v.Insumo);

                return Ok(resultados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar productos con término {Termino}", termino);
                return StatusCode(500, new { error = "Error interno del servidor al buscar productos" });
            }
        }

        /// <summary>
        /// Obtiene estadísticas por estado de stock
        /// </summary>
        [HttpGet("estadisticas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> GetEstadisticas()
        {
            try
            {
                _logger.LogInformation("Obteniendo estadísticas del inventario");
                var inventario = await _inventarioService.ObtenerInventarioTotalAsync();

                var estadisticas = new
                {
                    PorEstado = inventario
                        .GroupBy(v => v.EstadoStock)
                        .Select(g => new
                        {
                            Estado = g.Key,
                            Cantidad = g.Count(),
                            Porcentaje = Math.Round((double)g.Count() / inventario.Count() * 100, 2),
                            ValorTotal = g.Sum(v => v.Total)
                        })
                        .OrderByDescending(x => x.Cantidad),

                    TopProductosValor = inventario
                        .OrderByDescending(v => v.Total)
                        .Take(10)
                        .Select(v => new { v.Insumo, v.Total }),

                    ProductosSinMovimiento = inventario
                        .Where(v => v.Entrada == 0 && v.Salida == 0)
                        .Count()
                };

                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas del inventario");
                return StatusCode(500, new { error = "Error interno del servidor al obtener estadísticas" });
            }
        }
    }
}