using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models.Delison;
using Warehouse.Service.Delison;

namespace Warehouse.Controllers.Delison
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MoliendaController : ControllerBase
    {
        private readonly IMoliendaDelisonService _service;
        private readonly ILogger<MoliendaController> _logger;

        public MoliendaController(IMoliendaDelisonService service, ILogger<MoliendaController> logger)
        {
            _service = service;
            _logger  = logger;
        }

        [HttpGet("byCompany/{idCompany}")]
        public async Task<ActionResult<List<MoliendaDelison>>> GetByCompany(int idCompany)
        {
            try
            {
                return Ok(await _service.GetByCompany(idCompany));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Molienda for company {Id}", idCompany);
                return StatusCode(500, "Error retrieving data.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MoliendaDelison>> GetById(int id)
        {
            var result = await _service.GetById(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<MoliendaDelison>> Create([FromBody] MoliendaDelison data)
        {
            try
            {
                var result = await _service.Create(data);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Molienda");
                return StatusCode(500, "Error creating record.");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MoliendaDelison>> Update(int id, [FromBody] MoliendaDelison data)
        {
            try
            {
                var result = await _service.Update(id, data);
                return result == null ? NotFound() : Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Molienda {Id}", id);
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
                _logger.LogError(ex, "Error deleting Molienda {Id}", id);
                return StatusCode(500, "Error deleting record.");
            }
        }
    }
}
