
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service;

namespace Warehouse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfigurationService _configService;
        private readonly ILogger<ConfigurationController> _logger;

        public ConfigurationController(IConfigurationService configService, ILogger<ConfigurationController> logger)
        {
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<List<Configuration>>> GetAll(int idRoot)
        {
            try
            {
                var configs = await _configService.GetAllConfigurations(idRoot);
                return Ok(configs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving configurations.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Configuration>> GetById(int id)
        {
            try
            {
                var config = await _configService.GetConfigurationById(id);
                if (config == null)
                {
                    return NotFound();
                }
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving configuration with ID {Id}.", id);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Configuration config)
        {
            try
            {
                await _configService.Save(config);
                return CreatedAtAction(nameof(GetById), new { id = config.Id }, config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating configuration.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Configuration config)
        {
            try
            {
                var success = await _configService.Update(id, config);
                if (!success)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating configuration with ID {Id}.", id);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _configService.Delete(id);
                if (!success)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting configuration with ID {Id}.", id);
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
