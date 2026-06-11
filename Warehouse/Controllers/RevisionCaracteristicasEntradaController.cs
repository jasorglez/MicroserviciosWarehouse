using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models.Delison;
using Warehouse.Service.Delison;

namespace Warehouse.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class RevisionCaracteristicasEntradaController : ControllerBase
{
    private readonly IRevisionCaracteristicasEntradaService _service;
    private readonly ILogger<RevisionCaracteristicasEntradaController> _logger;

    public RevisionCaracteristicasEntradaController(IRevisionCaracteristicasEntradaService service, ILogger<RevisionCaracteristicasEntradaController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger  = logger  ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("byEntrada/{idEntrada}")]
    public async Task<ActionResult<List<RevisionCaracteristicasEntradaDelison>>> GetByEntrada(int idEntrada)
    {
        try
        {
            return Ok(await _service.GetByEntrada(idEntrada));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving revisión for entrada {IdEntrada}", idEntrada);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RevisionCaracteristicasEntradaDelison>> GetById(int id)
    {
        var item = await _service.GetById(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<RevisionCaracteristicasEntradaDelison>> Create([FromBody] RevisionCaracteristicasEntradaDelison data)
    {
        try
        {
            var created = await _service.Create(data);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating revisión característica entrada");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RevisionCaracteristicasEntradaDelison>> Update(int id, [FromBody] RevisionCaracteristicasEntradaDelison data)
    {
        try
        {
            var updated = await _service.Update(id, data);
            return updated == null ? NotFound() : Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating revisión característica entrada {Id}", id);
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
            _logger.LogError(ex, "Error deleting revisión característica entrada {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
