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
    // GET api/ItemComments?documentType=REQ&idDocument=5&idProvider=3119
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string documentType, [FromQuery] int idDocument, [FromQuery] string? numArticle, [FromQuery] int? idProvider = null)
    {
        if (idProvider.HasValue)
        {
            var comments = await _service.GetAsync(documentType, idDocument, numArticle ?? "", idProvider);
            return Ok(comments);
        }
        if (string.IsNullOrEmpty(numArticle))
        {
            var flags = await _service.GetFlagsAsync(documentType, idDocument);
            return Ok(flags);
        }
        var articleComments = await _service.GetAsync(documentType, idDocument, numArticle);
        return Ok(articleComments);
    }

    // GET api/ItemComments/count?documentType=MEDICION&idDocument=5
    [HttpGet("count")]
    public async Task<IActionResult> Count([FromQuery] string documentType, [FromQuery] int idDocument)
    {
        var count = await _service.CountAsync(documentType, idDocument);
        return Ok(new { count });
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

    // DELETE api/ItemComments/by-article?documentType=REQ&idDocument=5&numArticle=ABC-01
    // Borrado FÍSICO de comentarios de un artículo en un documento. Sin textPrefix borra todos
    // (al eliminar el renglón); con textPrefix borra solo los que empiezan así (reemplazo del panel).
    [HttpDelete("by-article")]
    public async Task<IActionResult> DeleteByArticle([FromQuery] string documentType, [FromQuery] int idDocument, [FromQuery] string numArticle, [FromQuery] string? textPrefix = null)
    {
        var deleted = await _service.DeleteByArticleAsync(documentType, idDocument, numArticle, textPrefix);
        return Ok(new { deleted });
    }
}

public class EditCommentDto
{
    public string Text { get; set; } = "";
}
