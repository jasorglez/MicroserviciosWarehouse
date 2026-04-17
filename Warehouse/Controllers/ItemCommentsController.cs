using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service;

namespace Warehouse.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ItemCommentsController : ControllerBase
{
    private readonly IItemCommentsService _service;

    public ItemCommentsController(IItemCommentsService service)
    {
        _service = service;
    }

    // GET api/ItemComments?documentType=REQ&idDocument=5&numArticle=ABC-01
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string documentType, [FromQuery] int idDocument, [FromQuery] string? numArticle)
    {
        if (string.IsNullOrEmpty(numArticle))
        {
            var flags = await _service.GetFlagsAsync(documentType, idDocument);
            return Ok(flags);
        }
        var comments = await _service.GetAsync(documentType, idDocument, numArticle);
        return Ok(comments);
    }

    // POST api/ItemComments
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ItemComment comment)
    {
        var created = await _service.AddAsync(comment);
        return Ok(created);
    }

    // PUT api/ItemComments/12
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] EditCommentDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto.Text);
        if (updated == null) return NotFound();
        return Ok(updated);
    }
}

public class EditCommentDto
{
    public string Text { get; set; } = "";
}
