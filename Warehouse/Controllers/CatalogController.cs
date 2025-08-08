using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Warehouse.Models;
using Warehouse.Service;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Warehouse.Models.DTOs;

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

        [HttpGet("getCatalogsAll")]
        public async Task<ActionResult<List<object>>> GetWarByComp(int idCompany)
        {
            try
            {
                var cat = await _catalogService.GetTypeAll(idCompany);
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
        public async Task<ActionResult<List<object>>> GetWarByComp(int idCompany, string type)
        {
            try
            {
                var cat = await _catalogService.GetType(type, idCompany);
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
        [HttpGet("getCatalogsVigente")]
        public async Task<ActionResult<List<object>>> GetWarByCompVigente(int idCompany, string type)
        {
            try
            {
                var cat = await _catalogService.GetTypeVigente(type, idCompany);
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

        [HttpGet("process-permissions")]
        public async Task<ActionResult<List<ProcessXPermission>>> GetProcessPermissions(int idProcces)
        {
            try
            {
                var permissions = await _catalogService.GetProcessXPermissions(idProcces);
                if (permissions == null || !permissions.Any())
                {
                    _logger.LogWarning("No ProcessXPermissions found for idProcces {idProcces}", idProcces);
                    return NotFound(new { Message = "No ProcessXPermissions Found", Permissions = new List<ProcessXPermission>() });
                }
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ProcessXPermissions for idProcces {idProcces}", idProcces);
                return StatusCode(500, "An error occurred while retrieving ProcessXPermissions.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Catalog cat)
        {
             if (!ModelState.IsValid)
              {
                 return BadRequest(ModelState);
              }
              try
              {
                   var success = await _catalogService.Update(id, cat);
                    if (success==null)
                    {
                        return NotFound($"Catalog with ID {id} not found");
                    }
                    return Ok(success);
              }              

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating catalog with ID {Id}.", id);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPut("update-permission/{idPerm}")]
        public async Task<IActionResult> UpdatePermiss(int idPerm, [FromBody] ProcessXPermission perm)
        {
            try
            {
                var success = await _catalogService.UpdatexPermission(idPerm, perm);
                if (!success)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating permission with ID {Id}.", idPerm);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("process-permissions")]
        public async Task<ActionResult> SaveProcessPermission([FromBody] ProcessXPermission perm)
        {
            try
            {
                await _catalogService.Savexpermission(perm);
                return Ok(new { Message = "New ProcessXPermission Created", Id = perm.Id, Permission = perm });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving ProcessXPermission");
                return StatusCode(500, "An error occurred while saving the ProcessXPermission.");
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
        public async Task<ActionResult> Create([FromBody] CatalogDTO cat)
        {
            try
            {
                var catalogDB = new  Catalog
                {
                    IdCompany = cat.IdCompany,
                    Description = cat.Description,
                    ValueAddition = cat.ValueAddition,
                    ValueAddition2 = cat.ValueAddition2,
                    ValueAdditionBit= cat.ValueAdditionBit,
                    ParentId = cat.ParentId,
                    SubParentId = cat.SubParentId,
                    Type = cat.Type,
                    Vigente = cat.Vigente,
                    Price = cat.Price,
                    Active = cat.Active
                };
                await _catalogService.Save(catalogDB);
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
                _logger.LogError(ex, "Error deleting Catalog with ID {Id}", id);
                return StatusCode(500, "An error occurred while deleting Catalog");
            }
        }
    }
}