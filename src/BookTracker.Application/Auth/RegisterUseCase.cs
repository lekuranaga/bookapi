using BookTracker.Application.Abstractions.Repositories;
using BookTracker.Application.Abstractions.Security;
using BookTracker.Application.Common;
using BookTracker.Domain.Entities;

namespace BookTracker.Application.Auth;

public sealed class RegisterUseCase
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _tokens;

    public RegisterUseCase(IUserRepository users, IPasswordHasher hasher, IJwtTokenGenerator tokens)
    {
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<AuthResponse> ExecuteAsync(RegisterRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var existing = await _users.FindByEmailAsync(email, ct);
        if (existing is not null)
            throw new ConflictException("Email already in use.");

        var user = User.Create(email, _hasher.Hash(request.Password));
        await _users.AddAsync(user, ct);

        var token = _tokens.Generate(user);
        return new AuthResponse(token.Token, token.ExpiresAt, user.Id, user.Email);
    }
}
