using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service;

namespace Warehouse.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class DetailsMoliendaController : ControllerBase
{
    private readonly IDetailsMoliendaService _service;
    private readonly ILogger<DetailsMoliendaController> _logger;

    public DetailsMoliendaController(IDetailsMoliendaService service, ILogger<DetailsMoliendaController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // GET api/DetailsMolienda/byMolienda/5?type=ENTRADA
    [HttpGet("byMolienda/{idMolienda}")]
    public async Task<ActionResult<List<DetailsMolienda>>> GetByMolienda(int idMolienda, [FromQuery] string type = "ENTRADA")
    {
        try
        {
            var items = await _service.GetByMoliendaAsync(idMolienda, type);
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving details for molienda {IdMolienda}", idMolienda);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DetailsMolienda>> GetById(int id)
    {
        try
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound($"Detail molienda with ID {id} not found.");
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving detail molienda with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<DetailsMolienda>> Create([FromBody] DetailsMolienda detail)
    {
        if (detail == null) return BadRequest("Detail cannot be null.");
        try
        {
            var created = await _service.CreateAsync(detail);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating detail molienda");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DetailsMolienda>> Update(int id, [FromBody] DetailsMolienda detail)
    {
        if (detail == null) return BadRequest("Detail cannot be null.");
        try
        {
            var updated = await _service.UpdateAsync(id, detail);
            if (updated == null) return NotFound($"Detail molienda with ID {id} not found.");
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating detail molienda with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            if (!result) return NotFound($"Detail molienda with ID {id} not found.");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting detail molienda with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
