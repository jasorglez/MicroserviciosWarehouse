using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Service;

namespace Warehouse.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LoyaltyProgramController : ControllerBase
{
    private readonly ILoyaltyProgramService _service;
    private readonly ILogger<LoyaltyProgramController> _logger;

    public LoyaltyProgramController(ILoyaltyProgramService service, ILogger<LoyaltyProgramController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet("byProduct")]
    public async Task<IActionResult> GetByProduct([FromQuery] int idCompany, [FromQuery] int idProduct)
    {
        try
        {
            var programs = await _service.GetByProductAsync(idCompany, idProduct);
            return Ok(programs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting loyalty programs for company {IdCompany} product {IdProduct}", idCompany, idProduct);
            return StatusCode(500, "Internal server error");
        }
    }
}
