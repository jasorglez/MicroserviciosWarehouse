using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models.Delison;
using Warehouse.Service.Delison;

namespace Warehouse.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ProductoTerminadoBomController : ControllerBase
{
    private readonly IProductoTerminadoBomService _service;
    private readonly ILogger<ProductoTerminadoBomController> _logger;

    public ProductoTerminadoBomController(IProductoTerminadoBomService service, ILogger<ProductoTerminadoBomController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger  = logger  ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("byCompany/{idCompany}")]
    public async Task<ActionResult<List<ProductoTerminadoBomDelison>>> GetByCompany(int idCompany)
    {
        try
        {
            return Ok(await _service.GetByCompany(idCompany));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving BOM for company {IdCompany}", idCompany);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("byRoot/{idCompany}/{idProductoRoot}")]
    public async Task<ActionResult<List<ProductoTerminadoBomDelison>>> GetByRoot(int idCompany, int idProductoRoot)
    {
        try
        {
            return Ok(await _service.GetByRoot(idCompany, idProductoRoot));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving BOM for company {IdCompany} root {IdRoot}", idCompany, idProductoRoot);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductoTerminadoBomDelison>> GetById(int id)
    {
        try
        {
            var item = await _service.GetById(id);
            if (item == null) return NotFound();
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving BOM node {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<ProductoTerminadoBomDelison>> Create([FromBody] ProductoTerminadoBomDelison data)
    {
        try
        {
            var created = await _service.Create(data);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating BOM node");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductoTerminadoBomDelison>> Update(int id, [FromBody] ProductoTerminadoBomDelison data)
    {
        try
        {
            var updated = await _service.Update(id, data);
            if (updated == null) return NotFound();
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating BOM node {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var ok = await _service.Delete(id);
            if (!ok) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting BOM node {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
