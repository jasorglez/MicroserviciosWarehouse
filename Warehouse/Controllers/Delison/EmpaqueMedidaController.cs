using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models.Delison;
using Warehouse.Models.DTOs;
using Warehouse.Service.Delison;

namespace Warehouse.Controllers.Delison
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmpaqueMedidaController : ControllerBase
    {
        private readonly IEmpaqueMedidaService _service;
        private readonly ILogger<EmpaqueMedidaController> _logger;

        public EmpaqueMedidaController(IEmpaqueMedidaService service, ILogger<EmpaqueMedidaController> logger)
        {
            _service = service;
            _logger  = logger;
        }

        // Medidas de una presentación (empaque_descripcion.id).
        [HttpGet("byEmpaque/{idEmpaque}")]
        public async Task<ActionResult<List<EmpaqueMedidaDelison>>> GetByEmpaque(int idEmpaque)
        {
            try { return Ok(await _service.GetByEmpaque(idEmpaque)); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting EmpaqueMedida for empaque {Id}", idEmpaque);
                return StatusCode(500, "Error retrieving data.");
            }
        }

        // Reemplazo bulk de las medidas de la presentación (Guardar único del Nivel 1/2).
        [HttpPost("save-by-empaque")]
        public async Task<ActionResult<List<EmpaqueMedidaDelison>>> SaveByEmpaque([FromBody] EmpaqueMedidaSaveDto dto)
        {
            try { return Ok(await _service.SaveByEmpaque(dto)); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving EmpaqueMedida for empaque {Id}", dto?.IdEmpaque);
                return StatusCode(500, "Error saving records.");
            }
        }
    }
}
