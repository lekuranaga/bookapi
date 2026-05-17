using System.Text.RegularExpressions;
using BookTracker.Domain.Exceptions;

namespace BookTracker.Domain.Entities;

public sealed class User
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public Guid Id { get; }
    public string Email { get; }
    public string PasswordHash { get; private set; }
    public DateTime CreatedAt { get; }

    private User(Guid id, string email, string passwordHash, DateTime createdAt)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = createdAt;
    }

    public static User Create(string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(email) || !EmailRegex.IsMatch(email))
            throw new DomainException("Invalid email.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("password hash is required.");

        return new User(Guid.NewGuid(), email.Trim().ToLowerInvariant(), passwordHash, DateTime.UtcNow);
    }

    public static User Hydrate(Guid id, string email, string passwordHash, DateTime createdAt)
        => new(id, email, passwordHash, createdAt);
}
