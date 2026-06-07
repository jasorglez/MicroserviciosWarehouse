using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models.Delison;
using Warehouse.Service.Delison;

namespace Warehouse.Controllers.Delison
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DescripcionEmpaqueController : ControllerBase
    {
        private readonly IDescripcionEmpaqueService _service;
        private readonly ILogger<DescripcionEmpaqueController> _logger;

        public DescripcionEmpaqueController(IDescripcionEmpaqueService service, ILogger<DescripcionEmpaqueController> logger)
        {
            _service = service;
            _logger  = logger;
        }

        [HttpGet("{idCompany}")]
        public async Task<ActionResult<List<DescripcionEmpaqueDelison>>> GetByCompany(int idCompany)
        {
            try
            {
                var result = await _service.GetByCompany(idCompany);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting DescripcionEmpaque for company {Id}", idCompany);
                return StatusCode(500, "Error retrieving data.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<DescripcionEmpaqueDelison>> Create([FromBody] DescripcionEmpaqueDelison data)
        {
            try
            {
                var result = await _service.Create(data);
                return CreatedAtAction(nameof(GetByCompany), new { idCompany = result.IdCompany }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating DescripcionEmpaque");
                return StatusCode(500, "Error creating record.");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DescripcionEmpaqueDelison>> Update(int id, [FromBody] DescripcionEmpaqueDelison data)
        {
            try
            {
                var result = await _service.Update(id, data);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating DescripcionEmpaque {Id}", id);
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
                _logger.LogError(ex, "Error deleting DescripcionEmpaque {Id}", id);
                return StatusCode(500, "Error deleting record.");
            }
        }
    }
}
