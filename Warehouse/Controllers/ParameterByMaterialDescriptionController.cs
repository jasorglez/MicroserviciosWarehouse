using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service;

namespace Warehouse.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ParameterByMaterialDescriptionController : ControllerBase
{
    private readonly IParameterByMaterialDescriptionService _service;
    private readonly ILogger<ParameterByMaterialDescriptionController> _logger;
    
    public ParameterByMaterialDescriptionController(IParameterByMaterialDescriptionService service, ILogger<ParameterByMaterialDescriptionController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /*[HttpGet("byIdCompany/{idCompany}/{idMaterial}")]
    public async Task<ActionResult<List<ParameterByMaterialDescription>>> GetParameterByMaterialDescriptions(int idCompany, int idMaterial)
    {
        try
        {
            var ParameterByMaterialDescriptions = await _service.GetParameterByMaterialDescriptionsAsync(idCompany, idMaterial);
            return Ok(ParameterByMaterialDescriptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving raw materials for company {IdCompany}", idCompany);
            return StatusCode(500, "Internal server error");
        }
    }*/

    [HttpGet("parameter/{idCompany}")]
    public async Task<ActionResult<List<Catalog>>> Getparameter(int idCompany)
    {
        try
        {
            var Catalogs = await _service.Getparameter(idCompany);
            return Ok(Catalogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parameters for company {IdCompany}", idCompany);
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPost]
    public async Task<ActionResult<ParameterByMaterialDescription>> CreateParameterByMaterialDescription([FromBody] ParameterByMaterialDescription parameter)
    {
        if (parameter == null)
        {
            return BadRequest("Parameter cannot be null.");
        }

        try
        {
            var createdParameterByMaterialDescription = await _service.CreateParameterByMaterialDescriptionAsync(parameter);
            return Ok(createdParameterByMaterialDescription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating raw material {parameter}" , parameter);
            return StatusCode(500, "Internal server error");
        }
    }
    
    /*[HttpPut]
    public async Task<ActionResult<ParameterByMaterialDescription>> UpdateParameterByMaterialDescription(int id, [FromBody] ParameterByMaterialDescription parameterByMaterialDescription)
    {
        if (parameterByMaterialDescription == null)
        {
            return BadRequest("Parameter cannot be null.");
        }

        try
        {
            var updatedParameterByMaterialDescription = await _service.UpdateParameterByMaterialDescriptionAsync(id, parameterByMaterialDescription);
            if (updatedParameterByMaterialDescription == null)
            {
                return NotFound($"Raw material with ID {id} not found.");
            }
            return Ok(updatedParameterByMaterialDescription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating raw material with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteParameterByMaterialDescription(int id)
    {
        try
        {
            var result = await _service.DeleteParameterByMaterialDescriptionAsync(id);
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
    }*/
}