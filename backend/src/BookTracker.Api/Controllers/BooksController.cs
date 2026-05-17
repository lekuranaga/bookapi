using BookTracker.Api.Validation;
using BookTracker.Application.Books;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/books")]
public sealed class BooksController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookDto>>> List(
        [FromServices] ListBooksUseCase useCase,
        CancellationToken ct)
        => Ok(await useCase.ExecuteAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookDto>> Get(
        Guid id,
        [FromServices] GetBookUseCase useCase,
        CancellationToken ct)
        => Ok(await useCase.ExecuteAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<BookDto>> Create(
        [FromBody] CreateBookRequest request,
        [FromServices] IValidator<CreateBookRequest> validator,
        [FromServices] CreateBookUseCase useCase,
        CancellationToken ct)
    {
        await validator.EnsureValidAsync(request, ct);
        var dto = await useCase.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BookDto>> Update(
        Guid id,
        [FromBody] UpdateBookRequest request,
        [FromServices] IValidator<UpdateBookRequest> validator,
        [FromServices] UpdateBookUseCase useCase,
        CancellationToken ct)
    {
        await validator.EnsureValidAsync(request, ct);
        return Ok(await useCase.ExecuteAsync(id, request, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromServices] DeleteBookUseCase useCase,
        CancellationToken ct)
    {
        await useCase.ExecuteAsync(id, ct);
        return NoContent();
    }
}
