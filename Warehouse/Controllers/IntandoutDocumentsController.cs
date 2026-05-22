using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service;

namespace Warehouse.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IntandoutDocumentsController : ControllerBase
    {
        private readonly IIntandoutDocumentsService _service;
        private readonly ILogger<IntandoutDocumentsController> _logger;

        public IntandoutDocumentsController(IIntandoutDocumentsService service, ILogger<IntandoutDocumentsController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> GetDocuments([FromQuery] int idDoc, [FromQuery] string type)
        {
            try
            {
                var result = await _service.GetDocuments(idDoc, type);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting intandout documents");
                return StatusCode(500, "An error occurred while retrieving intandout documents");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetById(id);
                if (result == null)
                {
                    return NotFound($"Document with id {id} not found");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting intandout document with ID {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the intandout document");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] IntandoutDocuments document)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _service.Save(document);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving intandout document");
                return StatusCode(500, "An error occurred while saving the intandout document");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] IntandoutDocuments document)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _service.Update(id, document);
                if (result == null)
                {
                    return NotFound($"Document with id {id} not found");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating intandout document with ID {Id}", id);
                return StatusCode(500, "An error occurred while updating the intandout document");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.Delete(id);
                if (!result)
                {
                    return NotFound($"Document with id {id} not found");
                }

                return Ok(new { Message = "Document deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting intandout document with ID {Id}", id);
                return StatusCode(500, "An error occurred while deleting the intandout document");
            }
        }
    }
}
