using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service;

namespace Warehouse.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class RawMaterialDetailsController : ControllerBase
{
    private readonly IRawMaterialDetailsService _service;
    private readonly ILogger<RawMaterialDetailsController> _logger;

    public RawMaterialDetailsController(IRawMaterialDetailsService service,
        ILogger<RawMaterialDetailsController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    [HttpGet("byRawMaterial/{idRawMaterial}")]
    public async Task<ActionResult<List<RawMaterialDetails>>> GetRawMaterialDetails(int idRawMaterial)
    {
        try
        {
            var rawMaterialDetails = await _service.GetRawMaterialDetailsAsync(idRawMaterial);
            return Ok(rawMaterialDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving raw material details for raw material ID {IdRawMaterial}", idRawMaterial);
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPost]
    public async Task<ActionResult<RawMaterialDetails>> CreateRawMaterialDetails([FromBody] RawMaterialDetails rawMaterialDetails)
    {
        if (rawMaterialDetails == null)
        {
            return BadRequest("Raw material details cannot be null.");
        }

        try
        {
            var createdDetails = await _service.CreateRawMaterialDetailsAsync(rawMaterialDetails);
            return CreatedAtAction(nameof(GetRawMaterialDetails), new { idRawMaterial = createdDetails.IdRawMaterial }, createdDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating raw material details");
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult<RawMaterialDetails>> UpdateRawMaterialDetails(int id, [FromBody] RawMaterialDetails rawMaterialDetails)
    {
        if (rawMaterialDetails == null)
        {
            return BadRequest("Raw material details cannot be null.");
        }

        try
        {
            var updatedDetails = await _service.UpdateRawMaterialDetailsAsync(id, rawMaterialDetails);
            if (updatedDetails == null)
            {
                return NotFound($"Raw material details with ID {id} not found.");
            }
            return Ok(updatedDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating raw material details with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRawMaterialDetails(int id)
    {
        try
        {
            var deleted = await _service.DeleteRawMaterialDetailsAsync(id);
            if (!deleted)
            {
                return NotFound($"Raw material details with ID {id} not found.");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting raw material details with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<RawMaterialDetails>> GetRawMaterialDetailsById(int id)
    {
        try
        {
            var rawMaterialDetails = await _service.GetRawMaterialDetailsByIdAsync(id);
            if (rawMaterialDetails == null)
            {
                return NotFound($"Raw material details with ID {id} not found.");
            }
            return Ok(rawMaterialDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving raw material details with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}