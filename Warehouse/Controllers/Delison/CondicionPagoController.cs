using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models.Delison;
using Warehouse.Service.Delison;

namespace Warehouse.Controllers.Delison
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CondicionPagoController : ControllerBase
    {
        private readonly ICondicionPagoService _service;
        private readonly ILogger<CondicionPagoController> _logger;

        public CondicionPagoController(ICondicionPagoService service, ILogger<CondicionPagoController> logger)
        {
            _service = service;
            _logger  = logger;
        }

        [HttpGet("{idCompany}")]
        public async Task<ActionResult<List<CondicionPago>>> GetByCompany(int idCompany)
        {
            try
            {
                var result = await _service.GetByCompany(idCompany);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting CondicionPago for company {Id}", idCompany);
                return StatusCode(500, "Error retrieving data.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CondicionPago>> Create([FromBody] CondicionPago data)
        {
            try
            {
                var result = await _service.Create(data);
                return CreatedAtAction(nameof(GetByCompany), new { idCompany = result.IdCompany }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating CondicionPago");
                return StatusCode(500, "Error creating record.");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CondicionPago>> Update(int id, [FromBody] CondicionPago data)
        {
            try
            {
                var result = await _service.Update(id, data);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating CondicionPago {Id}", id);
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
                _logger.LogError(ex, "Error deleting CondicionPago {Id}", id);
                return StatusCode(500, "Error deleting record.");
            }
        }
    }
}
