
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace Warehouse.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialController : ControllerBase
    {
        private readonly IMaterialService _service;
        private readonly ILogger<MaterialController> _logger;

        public MaterialController(IMaterialService service, ILogger<MaterialController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("{idCompany}")]
        public async Task<ActionResult<List<object>>> GetSupplies(int idCompany)
        {
            try
            {
                return await _service.GetSupplies(idCompany);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplies for company {IdCompany}", idCompany);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{2fields}")]
        public async Task<ActionResult<List<object>>> Supplies(int idCompany)
        {
            try
            {
                return await _service.Get2Supplies(idCompany);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplies for company {IdCompany}", idCompany);
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPost]
        public async Task<ActionResult> Save([FromBody] Material material)
        {
            try
            {
                await _service.Save(material);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving material");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Material>> Update(int id, [FromBody] Material material)
        {
            try
            {
                var result = await _service.Update(id, material);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating material with ID {Id}", id);
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
    }
}