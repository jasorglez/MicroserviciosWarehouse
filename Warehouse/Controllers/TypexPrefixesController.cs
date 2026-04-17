using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service;

namespace Warehouse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TypexPrefixesController : ControllerBase
    {
        private readonly ITypexPrefixesService _service;
        private readonly ILogger<TypexPrefixesController> _logger;

        public TypexPrefixesController(ITypexPrefixesService service, ILogger<TypexPrefixesController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<List<TypexPrefixes>>> GetAll()
        {
            try
            {
                var prefixes = await _service.GetAll();
                return Ok(prefixes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving TypexPrefixes.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("{reqType}/{idReqType}")]
        public async Task<ActionResult<TypexPrefixes>> GetByReqTypeAndId(string reqType, int idReqType)
        {
            try
            {
                var prefix = await _service.GetByReqTypeAndId(reqType, idReqType);
                if (prefix == null)
                {
                    return NotFound();
                }
                return Ok(prefix);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving TypexPrefixes with ReqType {ReqType} and IdReqType {IdReqType}.", reqType, idReqType);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TypexPrefixes typexPrefixes)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _service.Save(typexPrefixes);
                return CreatedAtAction(nameof(GetByReqTypeAndId),
                    new { reqType = typexPrefixes.ReqType, idReqType = typexPrefixes.IdReqType },
                    typexPrefixes);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Attempted to create duplicate TypexPrefixes.");
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating TypexPrefixes.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPut("{reqType}/{idReqType}")]
        public async Task<IActionResult> Update(string reqType, int idReqType, [FromBody] TypexPrefixes typexPrefixes)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _service.Update(reqType, idReqType, typexPrefixes);
                if (!success)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating TypexPrefixes with ReqType {ReqType} and IdReqType {IdReqType}.", reqType, idReqType);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete("{reqType}/{idReqType}")]
        public async Task<IActionResult> Delete(string reqType, int idReqType)
        {
            try
            {
                var success = await _service.Delete(reqType, idReqType);
                if (!success)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting TypexPrefixes with ReqType {ReqType} and IdReqType {IdReqType}.", reqType, idReqType);
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
