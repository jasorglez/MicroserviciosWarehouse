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
    public class EmpaquePesoVolumenController : ControllerBase
    {
        private readonly IEmpaquePesoVolumenService _service;
        private readonly ILogger<EmpaquePesoVolumenController> _logger;

        public EmpaquePesoVolumenController(IEmpaquePesoVolumenService service, ILogger<EmpaquePesoVolumenController> logger)
        {
            _service = service;
            _logger  = logger;
        }

        [HttpGet("byEmpaque/{idEmpaque}")]
        public async Task<ActionResult<List<EmpaquePesoVolumenDelison>>> GetByEmpaque(int idEmpaque)
        {
            try { return Ok(await _service.GetByEmpaque(idEmpaque)); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting EmpaquePesoVolumen for empaque {Id}", idEmpaque);
                return StatusCode(500, "Error retrieving data.");
            }
        }

        [HttpPost("save-by-empaque")]
        public async Task<ActionResult<List<EmpaquePesoVolumenDelison>>> SaveByEmpaque([FromBody] EmpaquePesoVolumenSaveDto dto)
        {
            try { return Ok(await _service.SaveByEmpaque(dto)); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving EmpaquePesoVolumen for empaque {Id}", dto?.IdEmpaque);
                return StatusCode(500, "Error saving records.");
            }
        }
    }
}
