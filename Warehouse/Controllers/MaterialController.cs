
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Warehouse.Models.DTOs;

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
        public async Task<ActionResult> Save([FromBody] CreateMaterialDTO material)
        {
         
            var materialDB = new Material
            {
                IdCompany = material.IdCompany,
                Insumo = material.Insumo,
                Articulo = material.Articulo,
                BarCode = material.BarCode,
                IdFamilia = material.IdFamilia,
                IdSubfamilia = material.IdSubfamilia,
                IdMedida = material.IdMedida,
                IdUbication = material.IdUbication,
                Description = material.Description,
                Date = material.Date,
                AplicaResg = material.AplicaResg,
                CostoMN = material.CostoMN,
                CostoDLL = material.CostoDLL,
                VentaMN = material.VentaMN,
                VentaDLL = material.VentaDLL,
                StockMin = material.StockMin,
                StockMax = material.StockMax,
                Picture = material.Picture,
                TypeMaterial = material.TypeMaterial,
                Vigente = material.Vigente,
                Active = material.Active
            };
            
            try
            {
                await _service.Save(materialDB);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving material");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Material>> Update(int id, [FromBody] CreateMaterialDTO material)
        {
            var materialDB = new Material
            {
                Id = id, // Assign the ID from the route parameter
                IdCompany = material.IdCompany,
                Insumo = material.Insumo,
                Articulo = material.Articulo,
                BarCode = material.BarCode,
                IdFamilia = material.IdFamilia,
                IdSubfamilia = material.IdSubfamilia,
                IdMedida = material.IdMedida,
                IdUbication = material.IdUbication,
                Description = material.Description,
                Date = material.Date,
                AplicaResg = material.AplicaResg,
                CostoMN = material.CostoMN,
                CostoDLL = material.CostoDLL,
                VentaMN = material.VentaMN,
                VentaDLL = material.VentaDLL,
                StockMin = material.StockMin,
                StockMax = material.StockMax,
                Picture = material.Picture,
                TypeMaterial = material.TypeMaterial,
                Vigente = material.Vigente,
                Active = material.Active
            };
            
            try
            {
                var result = await _service.Update(id, materialDB);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating material with ID {Id}", id);
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