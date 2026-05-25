using Microsoft.AspNetCore.Mvc;
using Warehouse.Service;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace Warehouse.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialxFinalProductController : ControllerBase
    {
        private readonly IMaterialxFinalProductService _service;
        private readonly ILogger<MaterialxFinalProductController> _logger;

        public MaterialxFinalProductController(IMaterialxFinalProductService service, ILogger<MaterialxFinalProductController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("exists")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> ExistsByMaterialAndPresentation(int idMaterial, int idPresentation)
        {
            if (idMaterial <= 0 || idPresentation <= 0)
            {
                _logger.LogWarning("Invalid parameters: IdMaterial {IdMaterial}, IdPresentation {IdPresentation}", idMaterial, idPresentation);
                return BadRequest(new { message = "IdMaterial e IdPresentation deben ser mayores a 0" });
            }

            try
            {
                var exists = await _service.ExistsByMaterialAndPresentation(idMaterial, idPresentation);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence for IdMaterial {IdMaterial} and IdPresentation {IdPresentation}", idMaterial, idPresentation);
                return StatusCode(500, new { message = "Error al verificar la existencia del registro" });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> AddIfNotExists(int idMaterial, int idPresentation)
        {
            if (idMaterial <= 0 || idPresentation <= 0)
            {
                _logger.LogWarning("Invalid parameters: IdMaterial {IdMaterial}, IdPresentation {IdPresentation}", idMaterial, idPresentation);
                return BadRequest(new { message = "IdMaterial e IdPresentation deben ser mayores a 0" });
            }

            try
            {
                var created = await _service.AddIfNotExists(idMaterial, idPresentation);

                if (!created)
                {
                    return Conflict(new { message = "El registro ya existe" });
                }

                return CreatedAtAction(nameof(ExistsByMaterialAndPresentation),
                    new { idMaterial, idPresentation },
                    new { idMaterial, idPresentation, active = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding MaterialxFinalProduct for IdMaterial {IdMaterial} and IdPresentation {IdPresentation}", idMaterial, idPresentation);
                return StatusCode(500, new { message = "Error al crear el registro" });
            }
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(int idMaterial, int idPresentation)
        {
            if (idMaterial <= 0 || idPresentation <= 0)
            {
                _logger.LogWarning("Invalid parameters: IdMaterial {IdMaterial}, IdPresentation {IdPresentation}", idMaterial, idPresentation);
                return BadRequest(new { message = "IdMaterial e IdPresentation deben ser mayores a 0" });
            }

            try
            {
                var result = await _service.Delete(idMaterial, idPresentation);

                if (!result)
                {
                    return NotFound(new { message = "El registro no existe o ya fue eliminado" });
                }

                return Ok(new { message = "Registro eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting MaterialxFinalProduct for IdMaterial {IdMaterial} and IdPresentation {IdPresentation}", idMaterial, idPresentation);
                return StatusCode(500, new { message = "Error al eliminar el registro" });
            }
        }
    }
}