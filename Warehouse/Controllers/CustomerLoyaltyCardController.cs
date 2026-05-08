using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Service;

namespace Warehouse.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomerLoyaltyCardController : ControllerBase
{
    private readonly ICustomerLoyaltyCardService _service;
    private readonly ILogger<CustomerLoyaltyCardController> _logger;

    public CustomerLoyaltyCardController(ICustomerLoyaltyCardService service, ILogger<CustomerLoyaltyCardController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("addStamp")]
    public async Task<IActionResult> AddStamp([FromQuery] int idCustomer, [FromQuery] int idLoyaltyProgram)
    {
        try
        {
            var result = await _service.AddStampAsync(idCustomer, idLoyaltyProgram);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Loyalty program not found: {IdLoyaltyProgram}", idLoyaltyProgram);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding stamp for customer {IdCustomer} program {IdLoyaltyProgram}", idCustomer, idLoyaltyProgram);
            return StatusCode(500, "Internal server error");
        }
    }
}
