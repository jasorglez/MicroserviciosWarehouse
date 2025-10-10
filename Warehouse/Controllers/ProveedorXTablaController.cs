
using Microsoft.AspNetCore.Mvc;
using Warehouse.Service;
using Warehouse.Models;
using System.Threading.Tasks;

namespace Warehouse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProveedorXTablaController : ControllerBase
    {
        private readonly IProveedorXTablaService _service;

        public ProveedorXTablaController(IProveedorXTablaService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProveedorXTabla>>> GetAll(int idProveedor, string Type)
        {
            var proveedores = await _service.GetAll(idProveedor, Type);
            return Ok(proveedores);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProveedorXTabla>> GetById(int id)
        {
            var proveedor = await _service.GetById(id);
            if (proveedor == null) return NotFound();
            return Ok(proveedor);
        }

        [HttpGet("type/{type}")]
        public async Task<ActionResult<List<ProveedorXTabla>>> GetByType(string type)
        {
            var proveedores = await _service.GetByType(type);
            return Ok(proveedores);
        }

        [HttpPost]
        public async Task<ActionResult<ProveedorXTabla>> Create(ProveedorXTabla proveedor)
        {
            var created = await _service.Create(proveedor);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProveedorXTabla>> Update(int id, ProveedorXTabla proveedor)
        {
            var updated = await _service.Update(id, proveedor);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpPut("abonoTabla/{id}/{table}")]
        public async Task<ActionResult<ProveedorXTabla>> UpdateAbonoTabla(int id, int table)
        {
            var updated = await _service.UpdateAbonoTabla(id, table);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var result = await _service.Delete(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPatch("{id}/toggle-active")]
        public async Task<ActionResult> ToggleActive(int id)
        {
            var result = await _service.ToggleActive(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
