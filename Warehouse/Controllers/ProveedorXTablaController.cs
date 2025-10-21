using Microsoft.AspNetCore.Mvc;
using Warehouse.Service;
using Warehouse.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Warehouse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProveedorXTablaController : ControllerBase
    {
        private readonly IProveedorXTablaService _service;

        public ProveedorXTablaController(IProveedorXTablaService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProveedorXTabla>>> GetAllByProveedor(
            [FromQuery] int idProveedor,
            [FromQuery] string type)
        {
            if (idProveedor <= 0 || string.IsNullOrWhiteSpace(type))
            {
                return BadRequest("Invalid proveedor ID or type");
            }

            var proveedores = await _service.GetAll(idProveedor, type);
            return Ok(proveedores);
        }

        [HttpGet]
        [Route("by-material")]
        public async Task<ActionResult<IEnumerable<ProveedorXTabla>>> GetMatxProv(
            [FromQuery] int idMaterial,
            [FromQuery] string type)
        {
            if (idMaterial <= 0 || string.IsNullOrWhiteSpace(type))
            {
                return BadRequest("Invalid material ID or type");
            }

            var proveedores = await _service.GetProvxMat(idMaterial, type);
            return Ok(proveedores);
        }

        [HttpGet("{id:int}", Name = nameof(GetById))]
        public async Task<ActionResult<ProveedorXTabla>> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid ID");
            }

            var proveedor = await _service.GetById(id);
            return proveedor == null ? NotFound() : Ok(proveedor);
        }

        [HttpGet("type/{type}")]
        public async Task<ActionResult<IEnumerable<ProveedorXTabla>>> GetByType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                return BadRequest("Invalid type");
            }

            var proveedores = await _service.GetByType(type);
            return Ok(proveedores);
        }

        [HttpPost]
        public async Task<ActionResult<ProveedorXTabla>> Create([FromBody] ProveedorXTabla proveedor)
        {
            if (proveedor == null)
            {
                return BadRequest("Proveedor data is required");
            }

            var created = await _service.Create(proveedor);
            return CreatedAtRoute(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ProveedorXTabla>> Update(
            int id,
            [FromBody] ProveedorXTabla proveedor)
        {
            if (id <= 0 || proveedor == null)
            {
                return BadRequest("Invalid ID or proveedor data");
            }

            var updated = await _service.Update(id, proveedor);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpPut("abono-tabla/{id:int}/{table:int}")]
        public async Task<ActionResult<ProveedorXTabla>> UpdateAbonoTabla(int id, int table)
        {
            if (id <= 0 || table <= 0)
            {
                return BadRequest("Invalid ID or table");
            }

            var updated = await _service.UpdateAbonoTabla(id, table);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid ID");
            }

            var result = await _service.Delete(id);
            return result ? NoContent() : NotFound();
        }

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