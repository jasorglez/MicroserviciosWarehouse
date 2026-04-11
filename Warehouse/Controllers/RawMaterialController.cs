using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service;

namespace Warehouse.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class RawMaterialController : ControllerBase
{
    private readonly IRawMaterialService _service;
    private readonly ILogger<RawMaterialController> _logger;
    
    public RawMaterialController(IRawMaterialService service, ILogger<RawMaterialController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    [HttpGet("byIdCompany/{idCompany}")]
    public async Task<ActionResult<List<RawMaterial>>> GetRawMaterials(int idCompany)
    {
        try
        {
            var rawMaterials = await _service.GetRawMaterialsAsync(idCompany);
            return Ok(rawMaterials);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving raw materials for company {IdCompany}", idCompany);
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<List<RawMaterial>>> GetRawMaterialById(int id)
    {
        try
        {
            var rawMaterials = await _service.GetRawMaterialByIdAsync(id);
            if (rawMaterials == null || rawMaterials.Count == 0)
            {
                return NotFound($"Raw material with ID {id} not found.");
            }
            return Ok(rawMaterials);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving raw material with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPost]
    public async Task<ActionResult<RawMaterial>> CreateRawMaterial([FromBody] RawMaterial rawMaterial)
    {
        if (rawMaterial == null)
        {
            return BadRequest("Raw material cannot be null.");
        }

        try
        {
            var createdRawMaterial = await _service.CreateRawMaterialAsync(rawMaterial);
            return CreatedAtAction(nameof(GetRawMaterialById), new { id = createdRawMaterial.Id }, createdRawMaterial);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating raw material");
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPut]
    public async Task<ActionResult<RawMaterial>> UpdateRawMaterial(int id, [FromBody] RawMaterial rawMaterial)
    {
        if (rawMaterial == null)
        {
            return BadRequest("Raw material cannot be null.");
        }

        try
        {
            var updatedRawMaterial = await _service.UpdateRawMaterialAsync(id, rawMaterial);
            if (updatedRawMaterial == null)
            {
                return NotFound($"Raw material with ID {id} not found.");
            }
            return Ok(updatedRawMaterial);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating raw material with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRawMaterial(int id)
    {
        try
        {
            var result = await _service.DeleteRawMaterialAsync(id);
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