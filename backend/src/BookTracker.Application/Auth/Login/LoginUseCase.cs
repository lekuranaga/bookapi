using BookTracker.Application.Abstractions.Security;
using BookTracker.Application.Auth.Shared;
using BookTracker.Application.Common;
using BookTracker.Application.Common.Exceptions;
using BookTracker.Domain.Users;
using FluentValidation;

namespace BookTracker.Application.Auth.Login;

public sealed class LoginUseCase(
    IValidator<LoginRequest> validator,
    IUserRepository users,
    IPasswordHasher hasher,
    IJwtTokenGenerator tokens)
{
    public async Task<AuthResponse> ExecuteAsync(LoginRequest request, CancellationToken ct)
    {
        await validator.EnsureValidAsync(request, ct);

        if (!Email.TryCreate(request.Email, out var email))
            throw new UnauthorizedException("Invalid credentials.");

        var user = await users.FindByEmailAsync(email, ct);
        if (user is null || !hasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid credentials.");

        var token = tokens.Generate(user);
        return new AuthResponse(token.Token, token.ExpiresAt, user.Id, user.Email.Value);
    }
}
