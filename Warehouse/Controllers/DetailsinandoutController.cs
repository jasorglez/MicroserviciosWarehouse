using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service;

namespace Warehouse.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]

public class DetailsinandoutController : ControllerBase
{
    private readonly IDetailsinandoutService _service;
    private readonly ILogger<DetailsinandoutController> _logger;

    public DetailsinandoutController(IDetailsinandoutService service, ILogger<DetailsinandoutController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    [HttpGet("{idInandout}")]
    public async Task<IActionResult> GetDetails(int idInandout)
    {
        try
        {
            var result = await _service.GetDetails(idInandout);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DetailsinandoutController for movement {IdInandout}", idInandout);
            throw;
        }
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] Detailsinandout detail)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _service.Save(detail);
            return Ok(detail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DetailsinandoutController for Save");
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Detailsinandout detail)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _service.Update(id, detail);
            return Ok(detail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DetailsinandoutController for Update");
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _service.Delete(id);
            if (!result)
            {
                return NotFound($"Detail {id} not found");
            }

            return Ok(new { Message = "Detail deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DetailsinandoutController for Delete {Id}", id);
            return StatusCode(500, "Internal Server Error");
        }
    }
}