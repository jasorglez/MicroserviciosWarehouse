using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models.Delison;
using Warehouse.Service.Delison;

namespace Warehouse.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class EntradaMoliendaController : ControllerBase
{
    private readonly IEntradaMoliendaService _service;
    private readonly ILogger<EntradaMoliendaController> _logger;

    public EntradaMoliendaController(IEntradaMoliendaService service, ILogger<EntradaMoliendaController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger  = logger  ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("byOc/{idOc}")]
    public async Task<ActionResult<List<EntradaMoliendaDelison>>> GetByOc(int idOc)
    {
        try
        {
            return Ok(await _service.GetByOc(idOc));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entradas for OC {IdOc}", idOc);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("byOcAndMaterial/{idOc}/{idMaterial}")]
    public async Task<ActionResult<List<EntradaMoliendaDelison>>> GetByOcAndMaterial(int idOc, int idMaterial)
    {
        try
        {
            return Ok(await _service.GetByOcAndMaterial(idOc, idMaterial));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entradas for OC {IdOc} and material {IdMaterial}", idOc, idMaterial);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("byEntregaAndMaterial/{idEntrega}/{idMaterial}")]
    public async Task<ActionResult<List<EntradaMoliendaDelison>>> GetByEntregaAndMaterial(int idEntrega, int idMaterial)
    {
        try
        {
            return Ok(await _service.GetByEntregaAndMaterial(idEntrega, idMaterial));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entradas for Entrega {IdEntrega} and material {IdMaterial}", idEntrega, idMaterial);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EntradaMoliendaDelison>> GetById(int id)
    {
        var item = await _service.GetById(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<EntradaMoliendaDelison>> Create([FromBody] EntradaMoliendaDelison entrada)
    {
        try
        {
            var created = await _service.Create(entrada);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating entrada molienda");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<EntradaMoliendaDelison>> Update(int id, [FromBody] EntradaMoliendaDelison entrada)
    {
        try
        {
            var updated = await _service.Update(id, entrada);
            return updated == null ? NotFound() : Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entrada molienda {Id}", id);
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
            _logger.LogError(ex, "Error deleting entrada molienda {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
