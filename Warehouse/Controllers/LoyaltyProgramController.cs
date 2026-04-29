using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service;

namespace Warehouse.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LoyaltyProgramController : ControllerBase
    {
        private readonly ILoyaltyProgramService _service;
        private readonly ILogger<LoyaltyProgramController> _logger;

        public LoyaltyProgramController(ILoyaltyProgramService service, ILogger<LoyaltyProgramController> logger)
        {
            _service = service;
            _logger  = logger;
        }

        [AllowAnonymous]
        [HttpGet("byProduct")]
        public async Task<ActionResult<List<LoyaltyProgram>>> GetByProduct([FromQuery] int idCompany, [FromQuery] int idProduct)
        {
            try
            {
                var programs = await _service.GetByProductAsync(idCompany, idProduct);
                return Ok(programs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loyalty programs for product {IdProduct}", idProduct);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{idCompany}")]
        public async Task<ActionResult<List<LoyaltyProgram>>> GetAll(int idCompany)
        {
            try
            {
                return Ok(await _service.GetAllAsync(idCompany));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loyalty programs for company {IdCompany}", idCompany);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("detail/{id}")]
        public async Task<ActionResult<LoyaltyProgram>> GetById(int id)
        {
            try
            {
                var program = await _service.GetByIdAsync(id);
                if (program == null) return NotFound();
                return Ok(program);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loyalty program {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<LoyaltyProgram>> Create([FromBody] LoyaltyProgram program)
        {
            try
            {
                var created = await _service.CreateAsync(program);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating loyalty program");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<LoyaltyProgram>> Update(int id, [FromBody] LoyaltyProgram program)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, program);
                if (updated == null) return NotFound();
                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating loyalty program {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                if (!result) return NotFound();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting loyalty program {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
