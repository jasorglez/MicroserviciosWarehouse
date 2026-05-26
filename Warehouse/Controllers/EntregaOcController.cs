using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models.Delison;
using Warehouse.Service.Delison;

namespace Warehouse.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class EntregaOcController : ControllerBase
{
    private readonly IEntregaOcService _service;
    private readonly ILogger<EntregaOcController> _logger;

    public EntregaOcController(IEntregaOcService service, ILogger<EntregaOcController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger  = logger  ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("byDetail/{idDetailsreqoc}")]
    public async Task<ActionResult<List<EntregaOcDelison>>> GetByDetail(int idDetailsreqoc)
    {
        try
        {
            return Ok(await _service.GetByDetail(idDetailsreqoc));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entregas for detail {IdDetailsreqoc}", idDetailsreqoc);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EntregaOcDelison>> GetById(int id)
    {
        var item = await _service.GetById(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<EntregaOcDelison>> Create([FromBody] EntregaOcDelison entrega)
    {
        try
        {
            var created = await _service.Create(entrega);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating entrega oc");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<EntregaOcDelison>> Update(int id, [FromBody] EntregaOcDelison entrega)
    {
        try
        {
            var updated = await _service.Update(id, entrega);
            return updated == null ? NotFound() : Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entrega oc {Id}", id);
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
            _logger.LogError(ex, "Error deleting entrega oc {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
