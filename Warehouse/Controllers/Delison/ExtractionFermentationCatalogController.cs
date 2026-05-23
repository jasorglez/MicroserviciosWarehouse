using Microsoft.AspNetCore.Mvc;
using Warehouse.Models.Delison;
using Warehouse.Service.Delison;

namespace Warehouse.Controllers.Delison
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExtractionFermentationCatalogController : ControllerBase
    {
        private readonly IExtractionFermentationCatalogService _service;
        private readonly ILogger<ExtractionFermentationCatalogController> _logger;

        public ExtractionFermentationCatalogController(
            IExtractionFermentationCatalogService service,
            ILogger<ExtractionFermentationCatalogController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int idCompany, [FromQuery] int? idBranch)
        {
            try
            {
                if (idBranch.HasValue)
                {
                    var result = await _service.GetByBranch(idCompany, idBranch.Value);
                    return Ok(result);
                }
                else
                {
                    var result = await _service.GetAll(idCompany);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ExtractionFermentationCatalog for company {IdCompany}", idCompany);
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
                _logger.LogError(ex, "Error retrieving ExtractionFermentationCatalog {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ExtractionFermentationCatalog entity)
        {
            try
            {
                var created = await _service.Create(entity);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ExtractionFermentationCatalog");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ExtractionFermentationCatalog entity)
        {
            try
            {
                var updated = await _service.Update(id, entity);
                if (updated == null) return NotFound();

                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ExtractionFermentationCatalog {Id}", id);
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
                _logger.LogError(ex, "Error deleting ExtractionFermentationCatalog {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
