
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Warehouse.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SetupController : ControllerBase
    {
        private readonly ISetupService _setupService;

        public SetupController(ISetupService setupService)
        {
            _setupService = setupService;
        }

        // GET: api/Setup/company/{idCompany}
        [HttpGet("company/{idCompany}")]
        public async Task<IActionResult> GetSetupsByCompany(int idCompany)
        {
            var setups = await _setupService.GetAllSetups(idCompany);

            if (setups == null || !setups.Any())
            {
                return NotFound(new { message = "No setups found for the specified company." });
            }

            return Ok(setups);
        }

        // GET: api/Setup/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSetupById(int id)
        {
            var setup = await _setupService.GetSetupById(id);

            if (setup == null)
            {
                return NotFound(new { message = $"Setup with ID {id} not found." });
            }

            return Ok(setup);
        }

        // POST: api/Setup
        [HttpPost]
        public async Task<IActionResult> CreateSetup([FromBody] Setup setup)
        {
            if (setup == null)
            {
                return BadRequest(new { message = "Setup data is required." });
            }

            await _setupService.Save(setup);
            return CreatedAtAction(nameof(GetSetupById), new { id = setup.Id }, setup);
        }

        // PUT: api/Setup/{idCompany}
        [HttpPut("{idCompany}")]
        public async Task<IActionResult> UpdateSetup(int idCompany, [FromBody] Setup setup)
        {
            if (setup == null)
            {
                return BadRequest(new { message = "Invalid setup data or ID mismatch." });
            }

            var result = await _setupService.Update(idCompany, setup);

            if (!result)
            {
                return NotFound(new { message = $"Setup with ID Company {idCompany} not found." });
            }

            return NoContent();
        }

        // DELETE: api/Setup/{idCompany}
        [HttpDelete("{idCompany}")]
        public async Task<IActionResult> DeleteSetup(int idCompany)
        {
            var result = await _setupService.Delete(idCompany);

            if (!result)
            {
                return NotFound(new { message = $"Setup with ID {idCompany} not found." });
            }

            return NoContent();
        }
    }
}