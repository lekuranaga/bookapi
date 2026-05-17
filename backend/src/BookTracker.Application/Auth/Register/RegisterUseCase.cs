using BookTracker.Application.Abstractions.Security;
using BookTracker.Application.Auth.Shared;
using BookTracker.Application.Common;
using BookTracker.Application.Common.Exceptions;
using BookTracker.Domain.Users;
using FluentValidation;

namespace BookTracker.Application.Auth.Register;

public sealed class RegisterUseCase(
    IValidator<RegisterRequest> validator,
    IUserRepository users,
    IPasswordHasher hasher,
    IJwtTokenGenerator tokens)
{
    public async Task<AuthResponse> ExecuteAsync(RegisterRequest request, CancellationToken ct)
    {
        await validator.EnsureValidAsync(request, ct);

        var email = Email.Create(request.Email);
        if (await users.FindByEmailAsync(email, ct) is not null)
            throw new ConflictException("Email already in use.");

        var user = User.Register(email, hasher.Hash(request.Password));
        await users.AddAsync(user, ct);

        var token = tokens.Generate(user);
        return new AuthResponse(token.Token, token.ExpiresAt, user.Id, user.Email.Value);
    }
}
