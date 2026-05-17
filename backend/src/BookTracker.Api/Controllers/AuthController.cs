using BookTracker.Api.Validation;
using BookTracker.Application.Auth;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace BookTracker.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request,
        [FromServices] IValidator<RegisterRequest> validator,
        [FromServices] RegisterUseCase useCase,
        CancellationToken ct)
    {
        await validator.EnsureValidAsync(request, ct);
        var response = await useCase.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(Register), response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        [FromServices] IValidator<LoginRequest> validator,
        [FromServices] LoginUseCase useCase,
        CancellationToken ct)
    {
        await validator.EnsureValidAsync(request, ct);
        return Ok(await useCase.ExecuteAsync(request, ct));
    }
}
