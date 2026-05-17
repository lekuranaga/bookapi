using BookTracker.Domain.Entities;

namespace BookTracker.Application.Abstractions.Security;

public sealed record AccessToken(string Token, DateTime ExpiresAt);

public interface IJwtTokenGenerator
{
    AccessToken Generate(User user);
}
