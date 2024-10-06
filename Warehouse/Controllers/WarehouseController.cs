using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service;

namespace Warehouse.Controllers
{
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

        [HttpGet("{idCompany}/{idProject}")]
        public async Task<ActionResult<List<object>>> GetWarByComp(string idCompany, string idProject)
        {
            try
            {
                var warehouses = await _warehouseService.GetWarehouses(idCompany, idProject);
                if (warehouses == null || warehouses.Count == 0)
                {
                    return NotFound();
                }
                return Ok(warehouses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving warehouses for company {IdCompany} and project {IdProject}", idCompany, idProject);
                return StatusCode(500, "An error occurred while retrieving warehouses.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Warehouset wh)
        {
            try
            {
                await _warehouseService.Save(wh);
                return CreatedAtAction(nameof(GetWarByComp), new { idCompany = wh.IdCompany, idProject = wh.IdProject }, wh);
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

    }
}
