using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service;

namespace Warehouse.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerLoyaltyCardController : ControllerBase
    {
        private readonly ICustomerLoyaltyCardService _service;
        private readonly ILogger<CustomerLoyaltyCardController> _logger;

        public CustomerLoyaltyCardController(ICustomerLoyaltyCardService service, ILogger<CustomerLoyaltyCardController> logger)
        {
            _service = service;
            _logger  = logger;
        }

        [HttpGet("byCustomer/{idCustomer}")]
        public async Task<ActionResult<List<CustomerLoyaltyCard>>> GetByCustomer(int idCustomer)
        {
            try
            {
                return Ok(await _service.GetByCustomerAsync(idCustomer));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cards for customer {IdCustomer}", idCustomer);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("byProgram/{idProgram}")]
        public async Task<ActionResult<List<CustomerLoyaltyCard>>> GetByProgram(int idProgram)
        {
            try
            {
                return Ok(await _service.GetByProgramAsync(idProgram));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cards for program {IdProgram}", idProgram);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("byCustomerAndProgram")]
        public async Task<ActionResult<CustomerLoyaltyCard>> GetByCustomerAndProgram(
            [FromQuery] int idCustomer, [FromQuery] int idLoyaltyProgram)
        {
            try
            {
                var card = await _service.GetByCustomerAndProgramAsync(idCustomer, idLoyaltyProgram);
                if (card == null) return NotFound();
                return Ok(card);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting card for customer {IdCustomer} program {IdProgram}", idCustomer, idLoyaltyProgram);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CustomerLoyaltyCard>> Create([FromBody] CustomerLoyaltyCard card)
        {
            try
            {
                var created = await _service.CreateAsync(card);
                return CreatedAtAction(nameof(GetByCustomerAndProgram),
                    new { idCustomer = created.IdCustomer, idLoyaltyProgram = created.IdLoyaltyProgram },
                    created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating loyalty card");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("addStamp")]
        public async Task<ActionResult<StampResult>> AddStamp(
            [FromQuery] int idCustomer, [FromQuery] int idLoyaltyProgram)
        {
            try
            {
                var result = await _service.AddStampAsync(idCustomer, idLoyaltyProgram);
                if (result == null) return NotFound("Programa de lealtad no encontrado");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding stamp for customer {IdCustomer} program {IdProgram}", idCustomer, idLoyaltyProgram);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
