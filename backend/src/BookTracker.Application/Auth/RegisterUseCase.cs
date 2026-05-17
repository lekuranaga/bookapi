using BookTracker.Application.Abstractions.Security;
using BookTracker.Application.Common;
using BookTracker.Domain.Users;

namespace BookTracker.Application.Auth;

public sealed class RegisterUseCase(IUserRepository users, IPasswordHasher hasher, IJwtTokenGenerator tokens)
{
    public async Task<AuthResponse> ExecuteAsync(RegisterRequest request, CancellationToken ct)
    {
        var email = Email.Create(request.Email);
        var existing = await users.FindByEmailAsync(email, ct);
        if (existing is not null)
            throw new ConflictException("Email already in use.");

        var user = User.Register(email, hasher.Hash(request.Password));
        await users.AddAsync(user, ct);

        var token = tokens.Generate(user);
        return new AuthResponse(token.Token, token.ExpiresAt, user.Id, user.Email.Value);
    }
}
