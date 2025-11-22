// ...existing code...
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service.Delison;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Warehouse.Models.DTOs;

namespace Warehouse.Controllers.Delison
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FamilySubFamilyDelisonController : ControllerBase
    {
        private readonly IFamilySubFamilyDelisonService _service;
        private readonly ILogger<FamilySubFamilyDelisonController> _logger;

        public FamilySubFamilyDelisonController(IFamilySubFamilyDelisonService service, ILogger<FamilySubFamilyDelisonController> logger)
        {
            _service = service;
            _logger = logger;
        }
        [HttpGet("Get")]
        public async Task<IActionResult> GetSupplies([FromQuery] int idCompany)
        {
            try
            {
                var supplies = await _service.GetSupplies(idCompany);
                return Ok(supplies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplies for company {idCompany}", idCompany);
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("GetDetailByMaster")]
        public async Task<IActionResult> GetDetailByMaster([FromQuery] int idCompany, [FromQuery] int idMasterFamily)
        {
            try
            {
                var supplies = await _service.GetDetailByMaster(idCompany, idMasterFamily);
                return Ok(supplies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplies for company {idCompany}", idCompany);
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("GetMasterFamily")]
        public async Task<IActionResult> GetSuppliesMasterFamily([FromQuery] int idCompany)
        {
            try
            {
                var supplies = await _service.GetSuppliesMasterFamily(idCompany);
                return Ok(supplies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplies for company {idCompany}", idCompany);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("GetCatalog")]
        public async Task<IActionResult> GetCatalog([FromQuery] int idCompany)
        {
            try
            {
                var supplies = await _service.GetCatalog(idCompany);
                return Ok(supplies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplies for company {idCompany}", idCompany);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("MasterFamily")]
        public async Task<IActionResult> SaveMasterFamily([FromBody] Warehouse.Service.Delison.Family family)
        {
            try
            {
                var supplies = await _service.SaveMasterFamily(family);
                return Ok(supplies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving");
                return StatusCode(500, "Internal server error");
            }
        }
        

        [HttpPut("MasterFamily")]
        public async Task<IActionResult> UpdateMasterFamily(int id, [FromBody] Warehouse.Service.Delison.Family family)
        {
            try
            {
                var supplies = await _service.UpdateMasterFamily(id, family);
                return Ok(supplies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplies for company {id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpPut("DetailMasterFamily")]
        public async Task<IActionResult> UpdateDetailMasterFamily([FromBody] Warehouse.Service.Delison.DetailFamily detailFamily)
        {
            try
            {
                var supplies = await _service.UpdateDetailMasterFamily(detailFamily);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplies for company {detailFamily}", detailFamily);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("MasterFamily/{id}")]
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



        // removed nested Family class to avoid type conflict with Warehouse.Service.Delison.Family


        public class Family
        {
            public int IdCompany { get; set; }
            public int MasterFamily { get; set; }
            public bool Vigente { get; set; }
        }

    }
}