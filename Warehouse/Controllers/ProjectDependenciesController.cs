using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Warehouse.Controllers;

[Authorize]
[ApiController]
[Route("api/project-dependencies")]
public class ProjectDependenciesController : ControllerBase
{
    private readonly DbWarehouseContext _context;

    public ProjectDependenciesController(DbWarehouseContext context) => _context = context;

    [HttpPost("presence")]
    public async Task<IActionResult> GetPresence([FromBody] List<int> projectIds)
    {
        var ids = (projectIds ?? new List<int>()).Where(id => id > 0).Distinct().ToArray();
        if (ids.Length > 1000)
            return BadRequest(new { message = "Solicita un máximo de 1000 proyectos." });
        if (ids.Length == 0)
            return Ok(Array.Empty<object>());

        var orders = await _context.Ocandreqs.AsNoTracking()
            .Where(row => row.Active == true && row.TypeReference == "project"
                && ids.Contains(row.IdReference) && (row.Type == "REQUIS" || row.Type == "OC"))
            .Select(row => new { row.IdReference, row.Type })
            .Distinct().ToListAsync();

        var movements = await _context.Inandouts.AsNoTracking()
            .Where(row => row.Active == true && row.IdProject.HasValue
                && ids.Contains(row.IdProject.Value) && (row.Type == "IN" || row.Type == "OUT"))
            .Select(row => new { row.IdProject, row.Type })
            .Distinct().ToListAsync();

        return Ok(ids.Select(id => new
        {
            idProject = id,
            hasRequisitions = orders.Any(row => row.IdReference == id && row.Type == "REQUIS"),
            hasPurchaseOrders = orders.Any(row => row.IdReference == id && row.Type == "OC"),
            hasEntries = movements.Any(row => row.IdProject == id && row.Type == "IN"),
            hasExits = movements.Any(row => row.IdProject == id && row.Type == "OUT")
        }));
    }
}
