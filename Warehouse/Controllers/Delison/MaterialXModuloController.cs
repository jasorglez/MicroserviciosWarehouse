using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models.Delison;
using Warehouse.Service.Delison;

namespace Warehouse.Controllers.Delison
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialXModuloController : ControllerBase
    {
        private readonly IMaterialXModuloService _service;
        private readonly ILogger<MaterialXModuloController> _logger;

        public MaterialXModuloController(IMaterialXModuloService service, ILogger<MaterialXModuloController> logger)
        {
            _service = service;
            _logger  = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int idCompany)
        {
            try
            {
                var result = await _service.GetAll(idCompany);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all MaterialXModulo for company {IdCompany}", idCompany);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("ByType")]
        public async Task<IActionResult> GetByType([FromQuery] int idCompany, [FromQuery] string type)
        {
            try
            {
                var result = await _service.GetByType(idCompany, type);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving MaterialXModulo by type {Type} for company {IdCompany}", type, idCompany);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("ByCatalog")]
        public async Task<IActionResult> GetByCatalog([FromQuery] int idCompany, [FromQuery] int idCatalog)
        {
            try
            {
                var result = await _service.GetByCatalog(idCompany, idCatalog);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving MaterialXModulo by catalog {IdCatalog} for company {IdCompany}", idCatalog, idCompany);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetById(id);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving MaterialXModulo {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MaterialXModulo entity)
        {
            try
            {
                var created = await _service.Create(entity);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MaterialXModulo");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MaterialXModulo entity)
        {
            try
            {
                var updated = await _service.Update(id, entity);
                if (updated == null) return NotFound();
                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating MaterialXModulo {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _service.Delete(id);
                if (!deleted) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting MaterialXModulo {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
