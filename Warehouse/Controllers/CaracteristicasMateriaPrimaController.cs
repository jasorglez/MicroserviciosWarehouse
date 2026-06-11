using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models.Delison;
using Warehouse.Service.Delison;

namespace Warehouse.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CaracteristicasMateriaPrimaController : ControllerBase
{
    private readonly ICaracteristicasMateriaPrimaService _service;
    private readonly ILogger<CaracteristicasMateriaPrimaController> _logger;

    public CaracteristicasMateriaPrimaController(ICaracteristicasMateriaPrimaService service, ILogger<CaracteristicasMateriaPrimaController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger  = logger  ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("byMaterial/{idMaterial}")]
    public async Task<ActionResult<List<CaracteristicasMateriaPrimaDelison>>> GetByMaterial(int idMaterial)
    {
        try
        {
            return Ok(await _service.GetByMaterial(idMaterial));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving características for material {IdMaterial}", idMaterial);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CaracteristicasMateriaPrimaDelison>> GetById(int id)
    {
        var item = await _service.GetById(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<CaracteristicasMateriaPrimaDelison>> Create([FromBody] CaracteristicasMateriaPrimaDelison data)
    {
        try
        {
            var created = await _service.Create(data);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating característica materia prima");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CaracteristicasMateriaPrimaDelison>> Update(int id, [FromBody] CaracteristicasMateriaPrimaDelison data)
    {
        try
        {
            var updated = await _service.Update(id, data);
            return updated == null ? NotFound() : Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating característica materia prima {Id}", id);
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
            _logger.LogError(ex, "Error deleting característica materia prima {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
