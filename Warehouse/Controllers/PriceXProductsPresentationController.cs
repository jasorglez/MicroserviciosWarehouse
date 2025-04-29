using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
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
    
    
    [HttpGet()]
    public async Task<ActionResult<List<object>>> GetPrices()
    {
        try
        {
            var pricesProducts = await _pricesXProductsPresentationService.GetPrices();
            if (pricesProducts.Count == 0)
            {
                return NotFound();
            }
            return Ok(pricesProducts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving warehouses for company ");
            return StatusCode(500, "An error occurred while retrieving warehouses.");
        }
    }
    
    [HttpGet("{idPrice}")]
    public async Task<ActionResult<object>> GetPricesById(int idPrice)
    {
        try
        {
            var pricesProduct = await _pricesXProductsPresentationService.GetPricesById(idPrice);
            if (pricesProduct == null)
            {
                return NotFound();
            }
            return Ok(pricesProduct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving warehouses for company ");
            return StatusCode(500, "An error occurred while retrieving warehouses.");
        }
    }
    
    
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] PricesXProductsPresentation wh)
    {
        try
        {
            await _pricesXProductsPresentationService.Save(wh);
            return CreatedAtAction(nameof(GetPricesById), new { idPrice = wh.Id  }, wh);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving warehouse");
            return StatusCode(500, "An error occurred while saving the warehouse.");
        }
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] PricesXProductsPresentation pricesXProductsPresentation)
    {
        try
        {
            var success = await _pricesXProductsPresentationService.Update(id, pricesXProductsPresentation);
            if (!success)
            {
                return NotFound();
            }
            
            return NoContent();
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating warehouse with ID {Id}", id);
            return StatusCode(500, "An error occurred while updating the warehouse.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var result = await _pricesXProductsPresentationService.Delete(id);
            if (!result)
            {
                return NotFound();
            }
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Warehouse with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
    
}