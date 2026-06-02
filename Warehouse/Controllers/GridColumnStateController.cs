using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models.Delison;
using Warehouse.Service.Delison;

namespace Warehouse.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class GridColumnStateController : ControllerBase
{
    private readonly IGridColumnStateService _service;
    private readonly ILogger<GridColumnStateController> _logger;

    public GridColumnStateController(IGridColumnStateService service, ILogger<GridColumnStateController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger  = logger  ?? throw new ArgumentNullException(nameof(logger));
    }

    // GET /api/GridColumnState?idUser=123&gridKey=gastos-captura  → 200 con el registro o null.
    [HttpGet]
    public async Task<ActionResult<GridColumnStateDelison>> Get([FromQuery] int idUser, [FromQuery] string gridKey)
    {
        try
        {
            if (idUser <= 0 || string.IsNullOrWhiteSpace(gridKey))
                return BadRequest("idUser y gridKey son requeridos.");

            return Ok(await _service.Get(idUser, gridKey));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting grid column state user {IdUser} key {GridKey}", idUser, gridKey);
            return StatusCode(500, "Internal server error");
        }
    }

    // POST /api/GridColumnState  (upsert por id_user + grid_key)
    [HttpPost]
    public async Task<ActionResult<GridColumnStateDelison>> Save([FromBody] GridColumnStateDelison data)
    {
        try
        {
            if (data == null || data.IdUser <= 0 || string.IsNullOrWhiteSpace(data.GridKey))
                return BadRequest("idUser y gridKey son requeridos.");

            return Ok(await _service.Save(data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving grid column state");
            return StatusCode(500, "Internal server error");
        }
    }
}
