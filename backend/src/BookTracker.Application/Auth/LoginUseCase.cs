using BookTracker.Application.Abstractions.Security;
using BookTracker.Application.Common;
using BookTracker.Domain.Common;
using BookTracker.Domain.Users;

namespace BookTracker.Application.Auth;

public sealed class LoginUseCase(IUserRepository users, IPasswordHasher hasher, IJwtTokenGenerator tokens)
{
    public async Task<AuthResponse> ExecuteAsync(LoginRequest request, CancellationToken ct)
    {
        Email email;
        try { email = Email.Create(request.Email); }
        catch (DomainException) { throw new UnauthorizedException("Invalid credentials."); }

        var user = await users.FindByEmailAsync(email, ct);
        if (user is null || !hasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid credentials.");

        var token = tokens.Generate(user);
        return new AuthResponse(token.Token, token.ExpiresAt, user.Id, user.Email.Value);
    }
}
