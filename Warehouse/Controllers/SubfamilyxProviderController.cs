using Microsoft.AspNetCore.Mvc;
using Warehouse.Service;
using Warehouse.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Warehouse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubfamilyxProviderController : ControllerBase
    {
        private readonly ISubfamilyxProviderService _service;
        private readonly ILogger<SubfamilyxProviderController> _logger;

        public SubfamilyxProviderController(
            ISubfamilyxProviderService service,
            ILogger<SubfamilyxProviderController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtener todos los registros activos por IdProvider
        /// </summary>
        /// <param name="idProvider">ID del proveedor</param>
        /// <returns>Lista de subfamilias asignadas al proveedor</returns>
        [HttpGet("provider/{idProvider:int}")]
        public async Task<ActionResult<IEnumerable<SubfamilyxProvider>>> GetByProvider(int idProvider)
        {
            if (idProvider <= 0)
            {
                return BadRequest("Invalid provider ID");
            }

            var subfamilies = await _service.GetByProvider(idProvider);
            return Ok(subfamilies);
        }

        /// <summary>
        /// Obtener un registro por Id
        /// </summary>
        /// <param name="id">ID del registro</param>
        /// <returns>Registro de SubfamilyxProvider</returns>
        [HttpGet("{id:int}", Name = nameof(GetSubfamilyxProviderById))]
        public async Task<ActionResult<SubfamilyxProvider>> GetSubfamilyxProviderById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid ID");
            }

            var record = await _service.GetById(id);
            return record == null ? NotFound() : Ok(record);
        }

        /// <summary>
        /// Obtener el registro principal de un proveedor
        /// </summary>
        /// <param name="idProvider">ID del proveedor</param>
        /// <returns>Registro principal</returns>
        [HttpGet("provider/{idProvider:int}/principal")]
        public async Task<ActionResult<SubfamilyxProvider>> GetPrincipal(int idProvider)
        {
            if (idProvider <= 0)
            {
                return BadRequest("Invalid provider ID");
            }

            var principal = await _service.GetPrincipalByProvider(idProvider);
            return principal == null ? NotFound() : Ok(principal);
        }

        /// <summary>
        /// Verificar si existe un registro activo
        /// </summary>
        /// <param name="idSubfamily">ID de la subfamilia</param>
        /// <param name="idProvider">ID del proveedor</param>
        /// <returns>True si existe</returns>
        [HttpGet("exists")]
        public async Task<ActionResult<bool>> Exists(
            [FromQuery] int idSubfamily,
            [FromQuery] int idProvider)
        {
            if (idSubfamily <= 0 || idProvider <= 0)
            {
                return BadRequest("Invalid subfamily or provider ID");
            }

            var exists = await _service.Exists(idSubfamily, idProvider);
            return Ok(exists);
        }

        /// <summary>
        /// Crear un nuevo registro
        /// </summary>
        /// <param name="subfamilyxProvider">Datos del registro a crear</param>
        /// <returns>Registro creado</returns>
        [HttpPost]
        public async Task<ActionResult<SubfamilyxProvider>> Create([FromBody] SubfamilyxProvider subfamilyxProvider)
        {
            if (subfamilyxProvider == null)
            {
                return BadRequest("SubfamilyxProvider data is required");
            }

            if (subfamilyxProvider.IdSubfamily <= 0 || subfamilyxProvider.IdProvider <= 0)
            {
                return BadRequest("Invalid IdSubfamily or IdProvider");
            }

            // Verificar si ya existe un registro activo
            var exists = await _service.Exists(subfamilyxProvider.IdSubfamily, subfamilyxProvider.IdProvider);
            if (exists)
            {
                return Conflict("This subfamily is already assigned to this provider");
            }

            var created = await _service.Create(subfamilyxProvider);
            return CreatedAtRoute(
                nameof(GetSubfamilyxProviderById),
                new { id = created.Id },
                created);
        }

        /// <summary>
        /// Actualizar un registro existente
        /// </summary>
        /// <param name="id">ID del registro</param>
        /// <param name="subfamilyxProvider">Datos actualizados</param>
        /// <returns>Registro actualizado</returns>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<SubfamilyxProvider>> Update(
            int id,
            [FromBody] SubfamilyxProvider subfamilyxProvider)
        {
            if (id <= 0 || subfamilyxProvider == null)
            {
                return BadRequest("Invalid ID or data");
            }

            if (subfamilyxProvider.IdSubfamily <= 0)
            {
                return BadRequest("Invalid IdSubfamily");
            }

            var updated = await _service.Update(id, subfamilyxProvider);
            return updated == null ? NotFound() : Ok(updated);
        }

        /// <summary>
        /// Eliminar (soft delete) por IdSubfamily e IdProvider
        /// </summary>
        /// <param name="idSubfamily">ID de la subfamilia</param>
        /// <param name="idProvider">ID del proveedor</param>
        /// <returns>NoContent si se eliminó correctamente</returns>
        [HttpDelete]
        public async Task<ActionResult> Delete(
            [FromQuery] int idSubfamily,
            [FromQuery] int idProvider)
        {
            if (idSubfamily <= 0 || idProvider <= 0)
            {
                return BadRequest("Invalid IdSubfamily or IdProvider");
            }

            var result = await _service.Delete(idSubfamily, idProvider);
            return result ? NoContent() : NotFound();
        }

        /// <summary>
        /// Eliminar (soft delete) por Id
        /// </summary>
        /// <param name="id">ID del registro</param>
        /// <returns>NoContent si se eliminó correctamente</returns>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid ID");
            }

            var result = await _service.DeleteById(id);
            return result ? NoContent() : NotFound();
        }

        /// <summary>
        /// Alternar estado Active
        /// </summary>
        /// <param name="id">ID del registro</param>
        /// <returns>NoContent si se actualizó correctamente</returns>
        [HttpPatch("{id:int}/toggle-active")]
        public async Task<ActionResult> ToggleActive(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid ID");
            }

            var result = await _service.ToggleActive(id);
            return result ? NoContent() : NotFound();
        }
    }
}
