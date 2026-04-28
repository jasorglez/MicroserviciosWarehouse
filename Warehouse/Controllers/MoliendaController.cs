using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models.Delison;
using Warehouse.Service.Delison;

namespace Warehouse.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class MoliendaController : ControllerBase
{
    private readonly IMoliendaDelisonService _service;
    private readonly ILogger<MoliendaController> _logger;

    public MoliendaController(IMoliendaDelisonService service, ILogger<MoliendaController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger  = logger  ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("byCompany/{idCompany}")]
    public async Task<ActionResult<List<MoliendaDelison>>> GetAll(int idCompany)
    {
        try
        {
            return Ok(await _service.GetByCompany(idCompany));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving molienda for company {IdCompany}", idCompany);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MoliendaDelison>> GetById(int id)
    {
        var item = await _service.GetById(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<MoliendaDelison>> Create([FromBody] MoliendaDelison molienda)
    {
        try
        {
            var created = await _service.Create(molienda);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating molienda record");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MoliendaDelison>> Update(int id, [FromBody] MoliendaDelison molienda)
    {
        try
        {
            var updated = await _service.Update(id, molienda);
            return updated == null ? NotFound() : Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating molienda with ID {Id}", id);
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
            _logger.LogError(ex, "Error deleting molienda with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
