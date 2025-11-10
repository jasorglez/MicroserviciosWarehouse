using Microsoft.AspNetCore.Mvc;
using Warehouse.Service;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace Warehouse.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FinalProductController : ControllerBase
    {
        private readonly IFinalProductService _service;
        private readonly ILogger<FinalProductController> _logger;

        public FinalProductController(IFinalProductService service, ILogger<FinalProductController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los productos finales por id de empresa
        /// </summary>
        /// <param name="idCompany">ID de la empresa</param>
        /// <returns>Lista de productos finales</returns>
        [HttpGet("company/{idCompany}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByCompanyId(int idCompany)
        {
            if (idCompany <= 0)
            {
                _logger.LogWarning("Invalid parameter: IdCompany {IdCompany}", idCompany);
                return BadRequest(new { message = "IdCompany debe ser mayor a 0" });
            }

            try
            {
                var products = await _service.GetByCompanyId(idCompany);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting final products for company {IdCompany}", idCompany);
                return StatusCode(500, new { message = "Error al obtener los productos finales" });
            }
        }
    }
}