using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.Authorization;
using Warehouse.Models;
using Warehouse.Service;

namespace Warehouse.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        private readonly ICatalogService _catalogService;
        private readonly ILogger<CatalogController> _logger;

        public CatalogController(ICatalogService catalogService, ILogger<CatalogController> logger)
        {
            _catalogService = catalogService ?? throw new ArgumentNullException(nameof(catalogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<List<object>>> GetWarByComp(string type, int idCompany)
        {
            try
            {
                var cat = await _catalogService.GetType(type,idCompany);
                if (cat == null || !cat.Any())
                {
                    _logger.LogWarning("No found Catalog the result is empty");
                    return NotFound(new { Message = "No Catalog Found or the result is empty", catalog = new List<object>() });
                }
                return Ok(cat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Catalog for Project");
                return StatusCode(500, "An error occurred while retrieving Catalog.");
            }
        }
        
        [HttpGet("getCatalogs")]
        public async Task<ActionResult<List<object>>> NewGetWarByComp(int idCompany, string type)
        {
            try
            {
                var cat = await _catalogService.NewGet(idCompany, type);
                if (cat == null || !cat.Any())
                {
                    _logger.LogWarning("No found Catalog the result is empty");
                    return NotFound(new { Message = "No Catalog Found or the result is empty", catalog = new List<object>() });
                }
                return Ok(cat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Catalog for Project");
                return StatusCode(500, "An error occurred while retrieving Catalog.");
            }
        }
        
        [HttpGet("family")]
        public async Task<ActionResult<List<object>>> GetFamilyCatalogs(int idCompany)
        {
            try
            {
                var catalogs = await _catalogService.GetFamilyCatalogs(idCompany);
                if (catalogs == null || catalogs.Count == 0)
                {
                    return NotFound();
                }
                return Ok(catalogs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving family catalogs");
                return StatusCode(500, "An error occurred while retrieving family catalogs.");
            }
        }
        
        [HttpGet("subfamily")]
        public async Task<ActionResult<List<object>>> GetSubfamilyCatalogs(int parentId)
        {
            try
            {
                var catalogs = await _catalogService.GetSubfamilyCatalogs(parentId);
                if (catalogs == null || catalogs.Count == 0)
                {
                    return NotFound();
                }
                return Ok(catalogs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subfamily catalogs");
                return StatusCode(500, "An error occurred while retrieving subfamily catalogs.");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] Catalog cat)
        {
            try
            {
                await _catalogService.Save(cat);
                return Ok(new { Message = "Record New with Id", id = cat.Id, Catalog = cat });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Catalog");
                return StatusCode(500, "An error occurred while creating the Catalog");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {

            try
            {
                var success = await _catalogService.Delete(id);
                if (success)
                {
                    return Ok(new { Message = "Delete Record with Id", id });
                }
                else
                {
                    return NotFound(new { Message = "Record Not Found with Id", id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Catalogs with ID {Id}", id);
                return StatusCode(500, "An error occurred while updating Catalogs");
            }
        }

    }
}
