using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service;

namespace Warehouse.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;
        private readonly ILogger<WarehouseController> _logger;

        public WarehouseController(IWarehouseService warehouseService, ILogger<WarehouseController> logger)
        {
            _warehouseService = warehouseService ?? throw new ArgumentNullException(nameof(warehouseService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet()]
        public async Task<ActionResult<List<object>>> GetWarByComp(int idBranch)
        {
            try
            {
                var warehouses = await _warehouseService.GetWarehouses(idBranch);
                if (warehouses == null || warehouses.Count == 0)
                {
                    return NotFound();
                }
                return Ok(warehouses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving warehouses for company {IdBranch}", idBranch);
                return StatusCode(500, "An error occurred while retrieving warehouses.");
            }
        }

        [HttpGet("2fields")]
        public async Task<ActionResult<List<object>>> War2fields(int idBranch)
        {
            try
            {
                var warehouses = await _warehouseService.Warehouses2fields(idBranch);
                if (warehouses == null || warehouses.Count == 0)
                {
                    return NotFound();
                }
                return Ok(warehouses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving warehouses for company {IdBranch}", idBranch);
                return StatusCode(500, "An error occurred while retrieving warehouses.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Warehouset wh)
        {
            try
            {
                await _warehouseService.Save(wh);
                return CreatedAtAction(nameof(GetWarByComp), new { idBranch = wh.IdBranch  }, wh);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving warehouse");
                return StatusCode(500, "An error occurred while saving the warehouse.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Warehouset warehouses)
        {
            try
            {
                var success = await _warehouseService.Update(id, warehouses);
                if (success)
                {
                    return NoContent();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating warehouse with ID {Id}", id);
                return StatusCode(500, "An error occurred while updating the warehouse.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var result = await _warehouseService.Delete(id);
                if (!result)
                    return NotFound();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Warehouse with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
