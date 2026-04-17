using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service;
using Microsoft.AspNetCore.Authorization;

namespace Warehouse.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PrefixSetupController : ControllerBase
    {
        private readonly IPrefixSetupService _prefixSetupService;

        public PrefixSetupController(IPrefixSetupService prefixSetupService)
        {
            _prefixSetupService = prefixSetupService;
        }

        // GET: api/PrefixSetup/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPrefixSetupById(int id)
        {
            var prefixSetup = await _prefixSetupService.GetPrefixSetupById(id);

            if (prefixSetup == null)
            {
                return NotFound(new { message = $"Prefix setup with ID {id} not found." });
            }

            return Ok(prefixSetup);
        }

        // GET: api/PrefixSetup/type/{type}/project-or-branch/{idProjectOrBranch}
        [HttpGet("type/{type}/project-or-branch/{idProjectOrBranch}")]
        public async Task<IActionResult> GetPrefixSetupByType(string type, int idProjectOrBranch)
        {
            var prefixSetup = await _prefixSetupService.GetPrefixSetupByType(type, idProjectOrBranch);

            if (prefixSetup == null)
            {
                return NotFound(new { message = $"Prefix setup with type '{type}' not found for project/branch ID {idProjectOrBranch}." });
            }

            return Ok(prefixSetup);
        }

        // POST: api/PrefixSetup
        [HttpPost]
        public async Task<IActionResult> CreatePrefixSetup([FromBody] PrefixSetup prefixSetup)
        {
            if (prefixSetup == null)
            {
                return BadRequest(new { message = "Prefix setup data is required." });
            }

            await _prefixSetupService.Save(prefixSetup);
            return CreatedAtAction(nameof(GetPrefixSetupById), new { id = prefixSetup.Id }, prefixSetup);
        }

        // PUT: api/PrefixSetup/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePrefixSetup(int id, [FromBody] PrefixSetup prefixSetup)
        {
            if (prefixSetup == null)
            {
                return BadRequest(new { message = "Invalid prefix setup data." });
            }

            var result = await _prefixSetupService.Update(id, prefixSetup);

            if (!result)
            {
                return NotFound(new { message = $"Prefix setup with ID {id} not found." });
            }

            return NoContent();
        }

        // DELETE: api/PrefixSetup/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrefixSetup(int id)
        {
            var result = await _prefixSetupService.Delete(id);

            if (!result)
            {
                return NotFound(new { message = $"Prefix setup with ID {id} not found." });
            }

            return NoContent();
        }
    }
}
