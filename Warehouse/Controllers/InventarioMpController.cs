using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Service.Delison;

namespace Warehouse.Controllers;

// Almacén GLOBAL de materia prima (solo lectura). Alimentado por las entradas
// liberadas en la Hoja de Gastos. Dos vistas según la selección del sidebar.
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class InventarioMpController : ControllerBase
{
    private readonly IInventarioMpService _service;
    private readonly ILogger<InventarioMpController> _logger;

    public InventarioMpController(IInventarioMpService service, ILogger<InventarioMpController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger  = logger  ?? throw new ArgumentNullException(nameof(logger));
    }

    // Vista GERENCIAL (sidebar = todas las sucursales): columnas = sucursales.
    [HttpGet("gerencial/{idCompany}")]
    public async Task<ActionResult<InventarioMpVistaDto>> GetGerencial(int idCompany)
    {
        try
        {
            return Ok(await _service.GetGerencial(idCompany));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inventario MP gerencial company {IdCompany}", idCompany);
            return StatusCode(500, "Internal server error");
        }
    }

    // Vista POR SUCURSAL: columnas = departamentos con datos.
    [HttpGet("porSucursal/{idCompany}/{idSucursal}")]
    public async Task<ActionResult<InventarioMpVistaDto>> GetPorSucursal(int idCompany, int idSucursal)
    {
        try
        {
            return Ok(await _service.GetPorSucursal(idCompany, idSucursal));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inventario MP por sucursal {IdSucursal} company {IdCompany}", idSucursal, idCompany);
            return StatusCode(500, "Internal server error");
        }
    }

    // Detalle de lotes de una celda (material × departamento × sucursal).
    [HttpGet("detalle/{idMaterial}/{idDepartamento}/{idSucursal}")]
    public async Task<ActionResult<InventarioMpDetalleDto>> GetDetalle(int idMaterial, int idDepartamento, int idSucursal)
    {
        try
        {
            return Ok(await _service.GetDetalle(idMaterial, idDepartamento, idSucursal));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detalle inventario MP material {IdMaterial} depto {IdDepto} sucursal {IdSuc}", idMaterial, idDepartamento, idSucursal);
            return StatusCode(500, "Internal server error");
        }
    }
}
