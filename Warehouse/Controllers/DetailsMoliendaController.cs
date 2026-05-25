using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models.Delison;
using Warehouse.Service.Delison;

namespace Warehouse.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class DetailsMoliendaController : ControllerBase
{
    private readonly IDetailsMoliendaDelisonService _service;
    private readonly ILogger<DetailsMoliendaController> _logger;

    public DetailsMoliendaController(IDetailsMoliendaDelisonService service, ILogger<DetailsMoliendaController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger  = logger  ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("byMolienda/{idMolienda}")]
    public async Task<ActionResult<List<DetailsMoliendaDelison>>> GetByMolienda(int idMolienda, [FromQuery] string? type)
    {
        try
        {
            return Ok(await _service.GetByMolienda(idMolienda, type));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving details for molienda {IdMolienda}", idMolienda);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DetailsMoliendaDelison>> GetById(int id)
    {
        var items = await _service.GetByMolienda(id, null);
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<DetailsMoliendaDelison>> Create([FromBody] DetailsMoliendaDelison detail)
    {
        try
        {
            var created = await _service.Create(detail);
            return StatusCode(201, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating detail molienda");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DetailsMoliendaDelison>> Update(int id, [FromBody] DetailsMoliendaDelison detail)
    {
        try
        {
            var updated = await _service.Update(id, detail);
            return updated == null ? NotFound() : Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating detail molienda with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _service.Delete(id);
            return result ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting detail molienda with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
