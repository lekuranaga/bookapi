namespace BookTracker.Application.Auth.Shared;

public sealed record AuthResponse(string AccessToken, DateTime ExpiresAt, Guid UserId, string Email);
