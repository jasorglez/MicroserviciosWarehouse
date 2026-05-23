using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service;

namespace Warehouse.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class MateriaByCatalogController : ControllerBase
{
    private readonly IMateriaByCatalogService _service;
    private readonly ILogger<MateriaByCatalogController> _logger;
    
    public MateriaByCatalogController(IMateriaByCatalogService service, ILogger<MateriaByCatalogController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    [HttpGet("byIdCompany/{idCompany}/{idMaterial}")]
    public async Task<ActionResult<List<MateriaByCatalog>>> GetMateriaByCatalogs(int idCompany, int idMaterial)
    {
        try
        {
            var MateriaByCatalogs = await _service.GetMateriaByCatalogsAsync(idCompany, idMaterial);
            return Ok(MateriaByCatalogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving raw materials for company {IdCompany}", idCompany);
            return StatusCode(500, "Internal server error");
        }
    }

    
    [HttpPost]
    public async Task<ActionResult<MateriaByCatalog>> CreateMateriaByCatalog([FromBody] MateriaByCatalog MateriaByCatalog)
    {
        if (MateriaByCatalog == null)
        {
            return BadRequest("Raw material cannot be null.");
        }

        try
        {
            var createdMateriaByCatalog = await _service.CreateMateriaByCatalogAsync(MateriaByCatalog);
            return Ok(createdMateriaByCatalog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating raw material");
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPut]
    public async Task<ActionResult<MateriaByCatalog>> UpdateMateriaByCatalog(int id, [FromBody] MateriaByCatalog MateriaByCatalog)
    {
        if (MateriaByCatalog == null)
        {
            return BadRequest("Raw material cannot be null.");
        }

        try
        {
            var updatedMateriaByCatalog = await _service.UpdateMateriaByCatalogAsync(id, MateriaByCatalog);
            if (updatedMateriaByCatalog == null)
            {
                return NotFound($"Raw material with ID {id} not found.");
            }
            return Ok(updatedMateriaByCatalog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating raw material with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMateriaByCatalog(int id)
    {
        try
        {
            var result = await _service.DeleteMateriaByCatalogAsync(id);
            if (!result)
            {
                return NotFound($"Raw material with ID {id} not found.");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting raw material with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}