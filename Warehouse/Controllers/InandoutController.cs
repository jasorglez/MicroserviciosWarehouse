using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Service;
using Warehouse.Models;

namespace Warehouse.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]

    public class InandoutController : ControllerBase
    {
        private readonly IInandoutService _service;
        private readonly ILogger<InandoutController> _logger;

        public InandoutController(IInandoutService service, ILogger<InandoutController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> GetInsAndOuts(int idProject, int IdWarehouse, string type)
        {
            try
            {
                var result = await _service.GetInsAndOuts(idProject, IdWarehouse, type);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ins and outs");
                return StatusCode(500, "An error occured while getting ins and outs");
            }
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInAndOutById(int id)
        {
            try
            {
                var result = await _service.GetInAndOutById(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Orders");
                return StatusCode(500, "An error occurred while retrieving the orders");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] Inandout inandout)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _service.Save(inandout);
                return Ok(inandout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving inandout");
                return StatusCode(500, "An error occured while saving inandout");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Inandout inandout)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _service.Update(id, inandout);
                if (result == null)
                {
                    return NotFound($"Order with id {id} not found");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inandout with ID {Id}", id);
                return StatusCode(500, "An error ocurred while updating the order");
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
                    return NotFound($"Order with id {id} not found");
                }

                return Ok(new { Message = "Order deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting inandout with ID {Id}", id);
                return StatusCode(500, "An error occured while deleting inandout");
            }
        }
    }
}



