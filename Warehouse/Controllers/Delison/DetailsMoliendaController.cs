using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models.Delison;
using Warehouse.Service.Delison;

namespace Warehouse.Controllers.Delison
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DetailsMoliendaController : ControllerBase
    {
        private readonly IDetailsMoliendaDelisonService _service;
        private readonly ILogger<DetailsMoliendaController> _logger;

        public DetailsMoliendaController(IDetailsMoliendaDelisonService service, ILogger<DetailsMoliendaController> logger)
        {
            _service = service;
            _logger  = logger;
        }

        [HttpGet("byMolienda/{idMolienda}")]
        public async Task<ActionResult<List<DetailsMoliendaDelison>>> GetByMolienda(int idMolienda, [FromQuery] string? type)
        {
            try
            {
                return Ok(await _service.GetByMolienda(idMolienda, type));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting DetailsMolienda for molienda {Id}", idMolienda);
                return StatusCode(500, "Error retrieving data.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<DetailsMoliendaDelison>> Create([FromBody] DetailsMoliendaDelison data)
        {
            try
            {
                var result = await _service.Create(data);
                return StatusCode(201, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating DetailsMolienda");
                return StatusCode(500, "Error creating record.");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DetailsMoliendaDelison>> Update(int id, [FromBody] DetailsMoliendaDelison data)
        {
            try
            {
                var result = await _service.Update(id, data);
                return result == null ? NotFound() : Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating DetailsMolienda {Id}", id);
                return StatusCode(500, "Error updating record.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _service.Delete(id);
                return deleted ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting DetailsMolienda {Id}", id);
                return StatusCode(500, "Error deleting record.");
            }
        }
    }
}
