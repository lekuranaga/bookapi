using Asp.Versioning;
using BookTracker.Application.Books.Create;
using BookTracker.Application.Books.Delete;
using BookTracker.Application.Books.Get;
using BookTracker.Application.Books.List;
using BookTracker.Application.Books.Shared;
using BookTracker.Application.Books.Update;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookTracker.Api.Controllers;

[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class BooksController(
    ListBooksUseCase list,
    GetBookUseCase get,
    CreateBookUseCase create,
    UpdateBookUseCase update,
    DeleteBookUseCase delete) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookDto>>> List(CancellationToken ct)
        => Ok(await list.ExecuteAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookDto>> Get(Guid id, CancellationToken ct)
        => Ok(await get.ExecuteAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<BookDto>> Create(CreateBookRequest request, CancellationToken ct)
    {
        var dto = await create.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = dto.Id, version = "1.0" }, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BookDto>> Update(Guid id, UpdateBookRequest request, CancellationToken ct)
        => Ok(await update.ExecuteAsync(id, request, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await delete.ExecuteAsync(id, ct);
        return NoContent();
    }
}
