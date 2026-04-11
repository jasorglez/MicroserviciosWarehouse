using Microsoft.AspNetCore.Mvc;
using Warehouse.Service;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace Warehouse.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProviderTypeController : ControllerBase
    {
        private readonly IProviderTypeService _service;
        private readonly ILogger<ProviderTypeController> _logger;

        public ProviderTypeController(IProviderTypeService service, ILogger<ProviderTypeController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los tipos de productos por id de proveedor
        /// </summary>
        /// <param name="idProvider">ID del proveedor</param>
        /// <returns>Lista de tipos de productos del proveedor</returns>
        [HttpGet("provider/{idProvider}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByProviderId(int idProvider)
        {
            if (idProvider <= 0)
            {
                _logger.LogWarning("Invalid parameter: IdProvider {IdProvider}", idProvider);
                return BadRequest(new { message = "IdProvider debe ser mayor a 0" });
            }

            try
            {
                var providerTypes = await _service.GetByProviderId(idProvider);
                return Ok(providerTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting provider types for provider {IdProvider}", idProvider);
                return StatusCode(500, new { message = "Error al obtener los tipos de proveedor" });
            }
        }
    }
}