using BookTracker.Application.Abstractions.Repositories;
using BookTracker.Application.Abstractions.Security;
using BookTracker.Application.Common;

namespace BookTracker.Application.Auth;

public sealed class LoginUseCase
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _tokens;

    public LoginUseCase(IUserRepository users, IPasswordHasher hasher, IJwtTokenGenerator tokens)
    {
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<AuthResponse> ExecuteAsync(LoginRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _users.FindByEmailAsync(email, ct);
        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid credentials.");

        var token = _tokens.Generate(user);
        return new AuthResponse(token.Token, token.ExpiresAt, user.Id, user.Email);
    }
}
