using BookTracker.Domain.Exceptions;

namespace BookTracker.Domain.Entities;

public sealed class Book
{
    public Guid Id { get; }
    public string Title { get; private set; }
    public string Author { get; private set; }
    public int Rating { get; private set; }
    public string Review { get; private set; }
    public DateOnly ReadAt { get; private set; }
    public Guid UserId { get; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; private set; }

    private Book(Guid id, string title, string author, int rating, string review, DateOnly readAt, Guid userId, DateTime createdAt, DateTime updatedAt)
    {
        Id = id;
        Title = title;
        Author = author;
        Rating = rating;
        Review = review;
        ReadAt = readAt;
        UserId = userId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Book Create(string title, string author, int rating, string? review, Guid userId, DateOnly readAt)
    {
        Validate(title, author, rating, userId);
        var now = DateTime.UtcNow;
        return new Book(Guid.NewGuid(), title.Trim(), author.Trim(), rating, review ?? string.Empty, readAt, userId, now, now);
    }

    public static Book Hydrate(Guid id, string title, string author, int rating, string review, DateOnly readAt, Guid userId, DateTime createdAt, DateTime updatedAt)
    {
        Validate(title, author, rating, userId);
        return new Book(id, title, author, rating, review, readAt, userId, createdAt, updatedAt);
    }

    public void Update(string title, string author, int rating, string? review, DateOnly readAt)
    {
        Validate(title, author, rating, UserId);
        Title = title.Trim();
        Author = author.Trim();
        Rating = rating;
        Review = review ?? string.Empty;
        ReadAt = readAt;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void Validate(string title, string author, int rating, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");
        if (string.IsNullOrWhiteSpace(author))
            throw new DomainException("Author is required.");
        if (rating < 1 || rating > 5)
            throw new DomainException("Rating must be between 1 and 5.");
        if (userId == Guid.Empty)
            throw new DomainException("UserId is required.");
    }
}
