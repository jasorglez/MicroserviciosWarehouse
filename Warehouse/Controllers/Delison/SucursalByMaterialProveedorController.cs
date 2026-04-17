// ...existing code...
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service.Delison;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Warehouse.Models.DTOs;
using Warehouse.Models.Delison;

namespace Warehouse.Controllers.Delison
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SucursalByMaterialProveedorController : ControllerBase
    {
        private readonly ISucursalByMaterialProveedorService _service;
        private readonly ILogger<SucursalByMaterialProveedorController> _logger;

        public SucursalByMaterialProveedorController(ISucursalByMaterialProveedorService service, ILogger<SucursalByMaterialProveedorController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] int idMaster)
        {
            try
            {
                var supplies = await _service.GetAll(idMaster);
                return Ok(supplies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplies for company {idMaster}", idMaster);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveMasterDetail([FromBody] SucursalByMaterialProveedor detail)
        {
            try
            {
                var supplies = await _service.SaveMasterDetail(detail);
                return Ok(supplies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving");
                return StatusCode(500, "Internal server error");
            }
        }
        

        [HttpPut]
        public async Task<IActionResult> UpdateMasterDetail(int id, [FromBody] SucursalByMaterialProveedor detail)
        {
            try
            {
                var supplies = await _service.UpdateMasterDetail(id, detail);
                return Ok(supplies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplies for company {id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.Delete(id);
                if (!result)
                    return NotFound();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting material with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }



        // removed nested Detail class to avoid type conflict with SucursalByMaterialProveedor


    }
}