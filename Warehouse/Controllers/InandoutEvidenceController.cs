using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service;

namespace Warehouse.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InandoutEvidenceController : ControllerBase
    {
        private readonly IInandoutEvidenceService _service;
        private readonly ILogger<InandoutEvidenceController> _logger;

        public InandoutEvidenceController(IInandoutEvidenceService service, ILogger<InandoutEvidenceController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("{idInandout}")]
        public async Task<IActionResult> GetByInandout(int idInandout)
        {
            try
            {
                var result = await _service.GetByInandout(idInandout);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting evidence for inandout {IdInandout}", idInandout);
                return StatusCode(500, "Error al consultar la evidencia fotográfica.");
            }
        }

        [HttpGet("counts")]
        public async Task<IActionResult> GetCounts([FromQuery] int[] idInandoutList)
        {
            try
            {
                var result = await _service.GetCountsByInandoutIds(idInandoutList ?? Array.Empty<int>());
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting evidence counts");
                return StatusCode(500, "Error al consultar los conteos de evidencia.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] Inandoutevidence evidence)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var saved = await _service.Save(evidence);
                return Ok(saved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving evidence");
                return StatusCode(500, "Error al guardar la evidencia fotográfica.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.Delete(id);
                if (!result) return NotFound();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting evidence {Id}", id);
                return StatusCode(500, "Error al eliminar la evidencia fotográfica.");
            }
        }
    }
}
