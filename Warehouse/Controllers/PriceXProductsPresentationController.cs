using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Models.DTOs;
using Warehouse.Service;

namespace Warehouse.Controllers;
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class PriceXProductsPresentationController : ControllerBase
{
    private readonly IPricesXProductsPresentationService _pricesXProductsPresentationService;
    private readonly ILogger<PriceXProductsPresentationController> _logger;

    public PriceXProductsPresentationController(IPricesXProductsPresentationService pricesXProductsPresentation, ILogger<PriceXProductsPresentationController> logger)
    {
        _pricesXProductsPresentationService = pricesXProductsPresentation ?? throw new ArgumentNullException(nameof(pricesXProductsPresentation));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PricesXProductsPresentationsDTO>>> GetPrices()
    {
        try
        {
            var pricesProducts = await _pricesXProductsPresentationService.GetPrices();

            if (!pricesProducts.Any())
            {
                return NotFound("No prices found.");
            }

            return Ok(pricesProducts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving prices for products presentation.");
            return StatusCode(500, "An error occurred while retrieving prices.");
        }
    }
    
    [HttpGet("{idPrice}")]
    public async Task<ActionResult<object>> GetPricesById(int idPrice)
    {
        try
        {
            // Llama al servicio para obtener el precio por ID
            var pricesProduct = await _pricesXProductsPresentationService.GetPricesById(idPrice);

            // Si no se encuentra el producto, devuelve NotFound (404)
            if (pricesProduct == null)
            {
                return NotFound(new { message = "Price Product not found." });
            }

            // Si se encuentra, retorna Ok(200) con el DTO de la respuesta
            return Ok(pricesProduct);
        }
        catch (Exception ex)
        {
            // Si hay un error en la ejecución, logea el error y retorna un 500
            _logger.LogError(ex, "Error retrieving price product by ID.");
            return StatusCode(500, "An error occurred while retrieving the price product.");
        }
    }
    
    
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreatePriceXProductsPresentationDTO dto)
    {
        try
        {
            var entity = new PricesXProductsPresentation()
            {
                IdMaterials = dto.IdMaterials,
                IdCatalogs = dto.IdCatalogs,
                Description = dto.Description,
                Price = dto.Price,
                Active = dto.Active
            };

            await _pricesXProductsPresentationService.Save(entity);

            return CreatedAtAction(nameof(GetPricesById), new { idPrice = entity.Id }, entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving price product");
            return StatusCode(500, "An error occurred while saving the price product.");
        }
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreatePriceXProductsPresentationDTO dto)
    {
        try
        {
            var success = await _pricesXProductsPresentationService.Update(id, dto);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating PricesXProductsPresentation with ID {Id}", id);
            return StatusCode(500, "An error occurred while updating the price record.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _pricesXProductsPresentationService.Delete(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent(); // 204 No Content es más apropiado para DELETE
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling PricesXProductsPresentation with ID {Id}", id);
            return StatusCode(500, "Internal server error while disabling the price record.");
        }
    }
    
}