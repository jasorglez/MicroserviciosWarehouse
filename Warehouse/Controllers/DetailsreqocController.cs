


using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Warehouse.Models.DTOs;

namespace Warehouse.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DetailsreqocController : ControllerBase
    {
        private readonly IDetailsreqocService _service;
        private readonly ILogger<DetailsreqocController> _logger;

        public DetailsreqocController(IDetailsreqocService service, ILogger<DetailsreqocController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("{idMovement}")]
        public async Task<IActionResult> GetDetails(int idMovement)
        {
            try
            {
                var result = await _service.GetDetails(idMovement);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Details for movement {IdMovement}", idMovement);
                return StatusCode(500, "An error occurred while retrieving the details");
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<PurchaseOrderDetail>>> GetPurchaseOrders(int idProv)
        {
            try
            {
                var result = await _service.GetPurchaseOrderDetailsQuery(idProv);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] Detailsreqoc detail)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _service.Save(detail);
                return Ok(detail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Detail");
                return StatusCode(500, "An error occurred while saving the detail");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Detailsreqoc detail)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _service.Update(id, detail);
                if (result == null)
                {
                    return NotFound($"Detail with ID {id} not found");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Detail with ID {Id}", id);
                return StatusCode(500, "An error occurred while updating the detail");
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
                    return NotFound($"Detail with ID {id} not found");
                }
                return Ok(new { Message = "Detail deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Detail with ID {Id}", id);
                return StatusCode(500, "An error occurred while deleting the detail");
            }
        }
    }
}