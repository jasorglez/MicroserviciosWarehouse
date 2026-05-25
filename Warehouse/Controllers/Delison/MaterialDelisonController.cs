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
    public class MaterialDelisonController : ControllerBase
    {
        private readonly IMaterialDelisonService _service;
        private readonly ILogger<MaterialDelisonController> _logger;

        public MaterialDelisonController(IMaterialDelisonService service, ILogger<MaterialDelisonController> logger)
        {
            _service = service;
            _logger = logger;
        }
        [HttpGet("Get")]
        public async Task<IActionResult> GetSupplies([FromQuery] int idCompany, [FromQuery] string type)
        {
            try
            {
                var supplies = await _service.GetSupplies(idCompany, type);
                return Ok(supplies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplies for company {idCompany} and type {type}", idCompany, type);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}