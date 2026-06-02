using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models.Delison;
using Warehouse.Service.Delison;

namespace Warehouse.Controllers.Delison
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SetupOcController : ControllerBase
    {
        private readonly ISetupOcService _service;
        private readonly ILogger<SetupOcController> _logger;

        public SetupOcController(ISetupOcService service, ILogger<SetupOcController> logger)
        {
            _service = service;
            _logger  = logger;
        }

        [HttpGet("{idCompany}")]
        public async Task<ActionResult<SetupOc>> GetByCompany(int idCompany)
        {
            try
            {
                var result = await _service.GetByCompany(idCompany);
                if (result == null) return NoContent();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting SetupOc for company {Id}", idCompany);
                return StatusCode(500, "Error retrieving data.");
            }
        }

        [HttpGet("branch/{idBranch}")]
        public async Task<ActionResult<SetupOc>> GetByBranch(int idBranch)
        {
            try
            {
                var result = await _service.GetByBranch(idBranch);
                if (result == null) return NoContent();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting SetupOc for branch {Id}", idBranch);
                return StatusCode(500, "Error retrieving data.");
            }
        }

        [HttpPost("{idCompany}")]
        public async Task<ActionResult<SetupOc>> CreateOrUpdate(int idCompany, [FromBody] SetupOc data)
        {
            try
            {
                var result = await _service.CreateOrUpdate(idCompany, data);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving SetupOc for company {Id}", idCompany);
                return StatusCode(500, "Error saving data.");
            }
        }

        [HttpPost("branch/{idBranch}")]
        public async Task<ActionResult<SetupOc>> CreateOrUpdateByBranch(int idBranch, [FromBody] SetupOc data)
        {
            try
            {
                var result = await _service.CreateOrUpdateByBranch(idBranch, data);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving SetupOc for branch {Id}", idBranch);
                return StatusCode(500, "Error saving data.");
            }
        }
    }
}
