using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service;

namespace Warehouse.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class MoliendaController : ControllerBase
{
    private readonly IMoliendaService _service;
    private readonly ILogger<MoliendaController> _logger;

    public MoliendaController(IMoliendaService service, ILogger<MoliendaController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("byCompany/{idCompany}")]
    public async Task<ActionResult<List<object>>> GetAll(int idCompany)
    {
        try
        {
            var items = await _service.GetAllAsync(idCompany);
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving molienda records for company {IdCompany}", idCompany);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Molienda>> GetById(int id)
    {
        try
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound($"Molienda with ID {id} not found.");
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving molienda with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Molienda>> Create([FromBody] Molienda molienda)
    {
        if (molienda == null) return BadRequest("Molienda cannot be null.");
        try
        {
            var created = await _service.CreateAsync(molienda);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating molienda record");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Molienda>> Update(int id, [FromBody] Molienda molienda)
    {
        if (molienda == null) return BadRequest("Molienda cannot be null.");
        try
        {
            var updated = await _service.UpdateAsync(id, molienda);
            if (updated == null) return NotFound($"Molienda with ID {id} not found.");
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating molienda with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            if (!result) return NotFound($"Molienda with ID {id} not found.");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting molienda with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
