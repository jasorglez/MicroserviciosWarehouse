using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models.Delison;
using Warehouse.Models.DTOs;
using Warehouse.Service.Delison;

namespace Warehouse.Controllers.Delison
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmpaqueDescripcionController : ControllerBase
    {
        private readonly IEmpaqueDescripcionService _service;
        private readonly ILogger<EmpaqueDescripcionController> _logger;

        public EmpaqueDescripcionController(IEmpaqueDescripcionService service, ILogger<EmpaqueDescripcionController> logger)
        {
            _service = service;
            _logger  = logger;
        }

        // Presentaciones de un par material-proveedor (proveedorxtablas.Id).
        [HttpGet("byProveedor/{idProveedorTabla}")]
        public async Task<ActionResult<List<EmpaqueDescripcionDelison>>> GetByProveedor(int idProveedorTabla)
        {
            try { return Ok(await _service.GetByProveedor(idProveedorTabla)); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting EmpaqueDescripcion for proveedorTabla {Id}", idProveedorTabla);
                return StatusCode(500, "Error retrieving data.");
            }
        }

        // Reemplazo bulk de las presentaciones (devuelve filas con ids para encadenar medidas/peso).
        [HttpPost("save-by-proveedor")]
        public async Task<ActionResult<List<EmpaqueDescripcionDelison>>> SaveByProveedor([FromBody] EmpaqueDescripcionSaveDto dto)
        {
            try { return Ok(await _service.SaveByProveedor(dto)); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving EmpaqueDescripcion for proveedorTabla {Id}", dto?.IdProveedorTabla);
                return StatusCode(500, "Error saving records.");
            }
        }

        // Panel de Requisiciones: proveedores del material con mínimo + presentaciones (base kg/L).
        [HttpGet("presentaciones-by-material/{idMaterial}")]
        public async Task<ActionResult<List<ProveedorPresentacionesDto>>> GetPresentacionesByMaterial(int idMaterial)
        {
            try { return Ok(await _service.GetPresentacionesByMaterial(idMaterial)); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting presentaciones for material {Id}", idMaterial);
                return StatusCode(500, "Error retrieving data.");
            }
        }
    }
}
