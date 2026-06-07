using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models.Delison;
using Warehouse.Service.Delison;

namespace Warehouse.Controllers.Delison
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PesoVolumenController : ControllerBase
    {
        private readonly IPesoVolumenService _service;
        private readonly ILogger<PesoVolumenController> _logger;

        public PesoVolumenController(IPesoVolumenService service, ILogger<PesoVolumenController> logger)
        {
            _service = service;
            _logger  = logger;
        }

        [HttpGet("{idCompany}")]
        public async Task<ActionResult<List<PesoVolumenDelison>>> GetByCompany(int idCompany)
        {
            try { return Ok(await _service.GetByCompany(idCompany)); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PesoVolumen for company {Id}", idCompany);
                return StatusCode(500, "Error retrieving data.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<PesoVolumenDelison>> Create([FromBody] PesoVolumenDelison data)
        {
            try
            {
                var result = await _service.Create(data);
                return CreatedAtAction(nameof(GetByCompany), new { idCompany = result.IdCompany }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PesoVolumen");
                return StatusCode(500, "Error creating record.");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PesoVolumenDelison>> Update(int id, [FromBody] PesoVolumenDelison data)
        {
            try
            {
                var result = await _service.Update(id, data);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating PesoVolumen {Id}", id);
                return StatusCode(500, "Error updating record.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _service.Delete(id);
                if (!deleted) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting PesoVolumen {Id}", id);
                return StatusCode(500, "Error deleting record.");
            }
        }
    }
}
