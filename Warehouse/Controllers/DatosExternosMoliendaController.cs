using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models.Delison;
using Warehouse.Service.Delison;

namespace Warehouse.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class DatosExternosMoliendaController : ControllerBase
{
    private readonly IDatosExternosMoliendaService _service;
    private readonly ILogger<DatosExternosMoliendaController> _logger;

    public DatosExternosMoliendaController(IDatosExternosMoliendaService service, ILogger<DatosExternosMoliendaController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger  = logger  ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("byEntrada/{idEntrada}")]
    public async Task<ActionResult<List<DatosExternosMoliendaDelison>>> GetByEntrada(int idEntrada)
    {
        try
        {
            return Ok(await _service.GetByEntrada(idEntrada));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving datos externos for entrada {IdEntrada}", idEntrada);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DatosExternosMoliendaDelison>> GetById(int id)
    {
        var item = await _service.GetById(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<DatosExternosMoliendaDelison>> Create([FromBody] DatosExternosMoliendaDelison data)
    {
        try
        {
            var created = await _service.Create(data);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating dato externo molienda");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DatosExternosMoliendaDelison>> Update(int id, [FromBody] DatosExternosMoliendaDelison data)
    {
        try
        {
            var updated = await _service.Update(id, data);
            return updated == null ? NotFound() : Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating dato externo molienda {Id}", id);
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
            _logger.LogError(ex, "Error deleting dato externo molienda {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
