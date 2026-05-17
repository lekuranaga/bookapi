using Asp.Versioning;
using BookTracker.Application.Auth.Login;
using BookTracker.Application.Auth.Register;
using BookTracker.Application.Auth.Shared;
using Microsoft.AspNetCore.Mvc;

namespace BookTracker.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class AuthController(RegisterUseCase register, LoginUseCase login) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var response = await register.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(Register), response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
        => Ok(await login.ExecuteAsync(request, ct));
}
