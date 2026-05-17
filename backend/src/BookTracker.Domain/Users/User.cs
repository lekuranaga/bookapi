using BookTracker.Domain.Common;

namespace BookTracker.Domain.Users;

public sealed class User : AggregateRoot
{
    public Email Email { get; }
    public string PasswordHash { get; }
    public DateTime CreatedAt { get; }

    private User(Guid id, Email email, string passwordHash, DateTime createdAt) : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = createdAt;
    }

    public static User Register(Email email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("password hash is required.");
        return new User(Guid.NewGuid(), email, passwordHash, DateTime.UtcNow);
    }

    public static User Hydrate(Guid id, string rawEmail, string passwordHash, DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("password hash is required.");
        return new User(id, Email.Create(rawEmail), passwordHash, createdAt);
    }
}
