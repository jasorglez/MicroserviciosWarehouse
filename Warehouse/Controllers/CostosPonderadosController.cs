using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Service.Delison;

namespace Warehouse.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CostosPonderadosController : ControllerBase
{
    private readonly ICostosPonderadosService _service;
    private readonly ILogger<CostosPonderadosController> _logger;

    public CostosPonderadosController(ICostosPonderadosService service, ILogger<CostosPonderadosController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger  = logger  ?? throw new ArgumentNullException(nameof(logger));
    }

    // Costos calculados de un material (KPIs + desglose por proveedor).
    [HttpGet("byMaterial/{idCompany}/{idMaterial}")]
    public async Task<ActionResult<CostoPonderadoDto>> GetByMaterial(int idCompany, int idMaterial, [FromQuery] int? ventana)
    {
        try
        {
            return Ok(await _service.GetByMaterial(idCompany, idMaterial, ventana));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error costos ponderados material {IdMaterial}", idMaterial);
            return StatusCode(500, "Internal server error");
        }
    }

    // Lote: costos de varios materiales a la vez (para los básicos de un producto en el BOM).
    [HttpPost("batch/{idCompany}")]
    public async Task<ActionResult<List<CostoPonderadoDto>>> GetBatch(int idCompany, [FromBody] List<int> idMaterials, [FromQuery] int? ventana)
    {
        try
        {
            return Ok(await _service.GetBatch(idCompany, idMaterials, ventana));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error costos ponderados batch company {IdCompany}", idCompany);
            return StatusCode(500, "Internal server error");
        }
    }

    // Ventana móvil (meses) configurable por empresa.
    [HttpGet("ventana/{idCompany}")]
    public async Task<ActionResult<object>> GetVentana(int idCompany)
    {
        try
        {
            return Ok(new { ventanaMeses = await _service.GetVentanaMeses(idCompany) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo ventana company {IdCompany}", idCompany);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("ventana/{idCompany}/{meses}")]
    public async Task<ActionResult<object>> SetVentana(int idCompany, int meses)
    {
        try
        {
            return Ok(new { ventanaMeses = await _service.SetVentanaMeses(idCompany, meses) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando ventana company {IdCompany}", idCompany);
            return StatusCode(500, "Internal server error");
        }
    }
}
