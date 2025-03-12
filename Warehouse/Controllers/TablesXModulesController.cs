
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Warehouse.Controllers
{
    [Authorize] // Ensures that only authenticated users can access these endpoints
    [Route("api/[controller]")] // Base route for the controller
    [ApiController]
    public class TablesXModulesController : ControllerBase
    {
        private readonly ITablesXModulesService _tablesXModulesService;

        public TablesXModulesController(ITablesXModulesService tablesXModulesService)
        {
            _tablesXModulesService = tablesXModulesService;
        }

        // GET: api/TablesXModules
        [HttpGet]
        public async Task<IActionResult> GetAllTablesXModules(string table)
        {
            var tablesXModules = await _tablesXModulesService.GetAllTablesXModules(table);

            if (tablesXModules == null || !tablesXModules.Any())
            {
                return NotFound(new { message = "No tablesxmodules records found." });
            }

            return Ok(tablesXModules);
        }

        // GET: api/TablesXModules/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTablesXModulesById(int id)
        {
            var tablesXModules = await _tablesXModulesService.GetTablesXModulesById(id);

            if (tablesXModules == null)
            {
                return NotFound(new { message = $"TablesXModules with ID {id} not found." });
            }

            return Ok(tablesXModules);
        }

        // POST: api/TablesXModules
        [HttpPost]
        public async Task<IActionResult> CreateTablesXModules([FromBody] TablesXModules tablesXModules)
        {
            if (tablesXModules == null)
            {
                return BadRequest(new { message = "TablesXModules data is required." });
            }

            await _tablesXModulesService.Save(tablesXModules);
            return CreatedAtAction(nameof(GetTablesXModulesById), new { id = tablesXModules.Id }, tablesXModules);
        }

        // PUT: api/TablesXModules/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTablesXModules(int id, [FromBody] TablesXModules tablesXModules)
        {
            if (tablesXModules == null || tablesXModules.Id != id)
            {
                return BadRequest(new { message = "Invalid tablesxmodules data or ID mismatch." });
            }

            var result = await _tablesXModulesService.Update(id, tablesXModules);

            if (!result)
            {
                return NotFound(new { message = $"TablesXModules with ID {id} not found." });
            }

            return NoContent();
        }

        // DELETE: api/TablesXModules/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTablesXModules(int id)
        {
            var result = await _tablesXModulesService.Delete(id);

            if (!result)
            {
                return NotFound(new { message = $"TablesXModules with ID {id} not found." });
            }

            return NoContent();
        }
    }
}