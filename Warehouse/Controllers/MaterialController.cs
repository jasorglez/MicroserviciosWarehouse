
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Warehouse.Models.DTOs;
using Warehouse.Models.Views;

namespace Warehouse.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialController : ControllerBase
    {
        private readonly IMaterialService _service;
        private readonly ILogger<MaterialController> _logger;

        public MaterialController(IMaterialService service, ILogger<MaterialController> logger)
        {
            _service = service;
            _logger = logger;
        }


        [HttpGet("{idCompany}")]
        public async Task<ActionResult<List<object>>> GetSupplies(int idCompany, string typematerial)
        {
            try
            {
                return await _service.GetSupplies(idCompany, typematerial);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplies for company {IdCompany}", idCompany);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("matprov/{idCompany}")]
        public async Task<ActionResult<List<ProveedoresxtypeView>>> GetMat(int idCompany)
        {
            try
            {
                return await _service.MaterialsxProvView(idCompany);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplies for company {IdCompany}", idCompany);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("materialsxproviders")]
        public async Task<ActionResult<List<MaterialsXProvider>>> GetMaterial(string Typereference, int idProv)
        {
            try
            {
                return await _service.MaterialsxProv(Typereference, idProv);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplies for company {IdProv}", idProv);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("with-counts/{idCompany}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<MaterialWithCount>>> GetMaterialsWithCounts(int idCompany)
        {
            if (idCompany <= 0)
            {
                _logger.LogWarning("Invalid company ID: {IdCompany}", idCompany);
                return BadRequest(new { message = "ID de compañía inválido" });
            }

            try
            {
                var materials = await _service.GetMaterialsWithCounts(idCompany);
                return Ok(materials);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting materials with counts for company {IdCompany}", idCompany);
                return StatusCode(500, new { message = "Error al obtener los materiales" });
            }
        }



        [HttpGet("2fields")]
        public async Task<ActionResult<List<object>>> Supplies(int idCompany)
        {
            try
            {
                return await _service.Get2Supplies(idCompany);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplies for company {IdCompany}", idCompany);
                return StatusCode(500, "Internal server error");
            }
        }

        
        [HttpGet("materialsview/{company}")]
        public async Task<ActionResult<List<MaterialsxProvExist>>> GetMaterialsView(int company)
        {
            try
            {
                return await _service.GetMaterialsView(company);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplies for company {company}", company);
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("byNameOrBarcode")]
        public async Task<ActionResult<List<object>>> GetSuppliesByNameOrBarCode(int idCompany, string nameOrBarCode)
        {
            try
            {
                var supplies = await _service.GetSuppliesByNameOrBarCode(idCompany, nameOrBarCode);
                if (supplies == null || supplies.Count == 0)
                {
                    return NotFound("No se encontraron productos con ese nombre o código de barras.");
                }
                return Ok(supplies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplies for company by name or code bar {NameOrCodeBar}", nameOrBarCode);
                return StatusCode(500, "Internal server error");
            }
        }



        [HttpPost]
        public async Task<IActionResult> CreateMaterial([FromBody] Material material)
            {
                try
                {
                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }

                    if (material == null)
                    {
                        return BadRequest("Material data is required");
                    }

                    await _service.Save(material);

                    return CreatedAtAction(nameof(CreateMaterial), new { id = material.Id }, material);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating material");
                    return StatusCode(500, "Internal server error");
                }
            }
        

        [HttpPut("{id}")]
        public async Task<ActionResult<Material>> Update(int id, [FromBody] Material material)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (material == null)
                {
                    return BadRequest("Material data is required");
                }

                var updateMat = await _service.Update(id,material);

                return Ok(updateMat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Update Material");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.Delete(id);
                if (!result)
                    return NotFound();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting material with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}