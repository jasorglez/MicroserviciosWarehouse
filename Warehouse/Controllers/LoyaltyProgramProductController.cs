using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using Warehouse.Service;

namespace Warehouse.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LoyaltyProgramProductController : ControllerBase
    {
        private readonly ILoyaltyProgramProductService _service;
        private readonly ILogger<LoyaltyProgramProductController> _logger;

        public LoyaltyProgramProductController(ILoyaltyProgramProductService service, ILogger<LoyaltyProgramProductController> logger)
        {
            _service = service;
            _logger  = logger;
        }

        [HttpPost]
        public async Task<ActionResult> AddProduct([FromBody] AddProductRequest request)
        {
            try
            {
                await _service.AddProductAsync(request.IdLoyaltyProgram, request.IdProduct);
                return Ok(new { message = "Product added to loyalty program" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product {IdProduct} to loyalty program {IdProgram}",
                    request.IdProduct, request.IdLoyaltyProgram);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{idProgram}/{idProduct}")]
        public async Task<ActionResult> RemoveProduct(int idProgram, int idProduct)
        {
            try
            {
                await _service.RemoveProductAsync(idProgram, idProduct);
                return Ok(new { message = "Product removed from loyalty program" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing product {IdProduct} from loyalty program {IdProgram}",
                    idProduct, idProgram);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("program/{idProgram}")]
        public async Task<ActionResult> ClearProducts(int idProgram)
        {
            try
            {
                await _service.ClearProductsAsync(idProgram);
                return Ok(new { message = "All products cleared from loyalty program" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing products from loyalty program {IdProgram}", idProgram);
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class AddProductRequest
    {
        [JsonPropertyName("idLoyaltyProgram")]
        public int IdLoyaltyProgram { get; set; }

        [JsonPropertyName("idProduct")]
        public int IdProduct { get; set; }
    }
}
