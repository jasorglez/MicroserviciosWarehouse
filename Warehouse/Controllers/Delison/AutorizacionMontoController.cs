using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models.Delison;
using Warehouse.Service.Delison;

namespace Warehouse.Controllers.Delison
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AutorizacionMontoController : ControllerBase
    {
        private readonly IAutorizacionMontoService _service;
        private readonly ILogger<AutorizacionMontoController> _logger;

        public AutorizacionMontoController(IAutorizacionMontoService service, ILogger<AutorizacionMontoController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("{idCompany}")]
        public async Task<ActionResult<List<AutorizacionMonto>>> GetByCompany(int idCompany)
        {
            try
            {
                var result = await _service.GetByCompany(idCompany);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AutorizacionMonto for company {Id}", idCompany);
                return StatusCode(500, "Error retrieving data.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<AutorizacionMonto>> Create([FromBody] AutorizacionMonto data)
        {
            try
            {
                var result = await _service.Create(data);
                return CreatedAtAction(nameof(GetByCompany), new { idCompany = result.IdCompany }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating AutorizacionMonto");
                return StatusCode(500, "Error creating record.");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AutorizacionMonto>> Update(int id, [FromBody] AutorizacionMonto data)
        {
            try
            {
                var result = await _service.Update(id, data);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating AutorizacionMonto {Id}", id);
                return StatusCode(500, "Error updating record.");
            }
        }
    }
}
