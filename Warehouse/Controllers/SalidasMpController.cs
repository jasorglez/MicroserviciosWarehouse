using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models.Delison;
using Warehouse.Service.Delison;

namespace Warehouse.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class SalidasMpController : ControllerBase
{
    private readonly ISalidasMpService _service;
    private readonly ILogger<SalidasMpController> _logger;

    public SalidasMpController(ISalidasMpService service, ILogger<SalidasMpController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger  = logger  ?? throw new ArgumentNullException(nameof(logger));
    }

    // Lotes disponibles para consumir (FEFO) un material en sucursal+departamento.
    [HttpGet("disponibles/{idMaterial}/{idDepartamento}/{idSucursal}")]
    public async Task<ActionResult<List<LoteDisponibleDto>>> GetLotesDisponibles(int idMaterial, int idDepartamento, int idSucursal)
    {
        try
        {
            return Ok(await _service.GetLotesDisponibles(idMaterial, idDepartamento, idSucursal));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error lotes disponibles material {IdMat} depto {IdDep} suc {IdSuc}", idMaterial, idDepartamento, idSucursal);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("byOrigen/{tipoOrigen}/{idOrigen}")]
    public async Task<ActionResult<List<SalidasMpDelison>>> GetByOrigen(string tipoOrigen, int idOrigen)
    {
        try
        {
            return Ok(await _service.GetByOrigen(tipoOrigen, idOrigen));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error salidas MP por origen {Tipo}/{Id}", tipoOrigen, idOrigen);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<SalidasMpDelison>> Create([FromBody] SalidasMpDelison data)
    {
        try
        {
            var created = await _service.Create(data);
            return Ok(created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando salida MP");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SalidasMpDelison>> Update(int id, [FromBody] SalidasMpDelison data)
    {
        try
        {
            var updated = await _service.Update(id, data);
            return updated == null ? NotFound() : Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando salida MP {Id}", id);
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
            _logger.LogError(ex, "Error eliminando salida MP {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
