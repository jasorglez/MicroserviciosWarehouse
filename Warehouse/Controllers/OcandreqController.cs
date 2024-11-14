
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service;

namespace Warehouse.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OcandreqController : ControllerBase
    {
        private readonly IOcandreqService _service;
        private readonly ILogger<OcandreqController> _logger;

        public OcandreqController(IOcandreqService service, ILogger<OcandreqController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders(int idProject)
        {
            try
            {
                var result = await _service.GetOrders(idProject);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Orders");
                return StatusCode(500, "An error occurred while retrieving the orders");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] Ocandreq ocandreq)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _service.Save(ocandreq);
                return Ok(ocandreq);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Order");
                return StatusCode(500, "An error occurred while saving the order");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Ocandreq ocandreq)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _service.Update(id, ocandreq);
                if (result == null)
                {
                    return NotFound($"Order with ID {id} not found");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Order with ID {Id}", id);
                return StatusCode(500, "An error occurred while updating the order");
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
                    return NotFound($"Order with ID {id} not found");
                }
                return Ok(new { Message = "Order deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Order with ID {Id}", id);
                return StatusCode(500, "An error occurred while deleting the order");
            }
        }
    }
}