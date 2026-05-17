using BookTracker.Domain.Common;

namespace BookTracker.Domain.Books;

public sealed class Book : AggregateRoot
{
    public const int MaxTitleLength = 200;
    public const int MaxAuthorLength = 150;
    public const int MaxReviewLength = 4000;

    public Guid UserId { get; }
    public string Title { get; private set; }
    public string Author { get; private set; }
    public Rating Rating { get; private set; }
    public string Review { get; private set; }
    public DateOnly ReadAt { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; private set; }

    private Book(Guid id, Guid userId, string title, string author, Rating rating, string review,
                 DateOnly readAt, DateTime createdAt, DateTime updatedAt) : base(id)
    {
        UserId = userId;
        Title = title;
        Author = author;
        Rating = rating;
        Review = review;
        ReadAt = readAt;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Book Log(Guid userId, string title, string author, Rating rating, string? review, DateOnly readAt)
    {
        EnsureOwner(userId);
        var (t, a, r) = NormalizeAndValidate(title, author, review);
        var now = DateTime.UtcNow;
        return new Book(Guid.NewGuid(), userId, t, a, rating, r, readAt, now, now);
    }

    public static Book Hydrate(Guid id, Guid userId, string title, string author, int rating, string review,
                                DateOnly readAt, DateTime createdAt, DateTime updatedAt)
    {
        EnsureOwner(userId);
        var (t, a, r) = NormalizeAndValidate(title, author, review);
        return new Book(id, userId, t, a, Rating.Of(rating), r, readAt, createdAt, updatedAt);
    }

    public void Revise(string title, string author, Rating rating, string? review, DateOnly readAt)
    {
        var (t, a, r) = NormalizeAndValidate(title, author, review);
        Title = t;
        Author = a;
        Rating = rating;
        Review = r;
        ReadAt = readAt;
        UpdatedAt = DateTime.UtcNow;
    }

    private static (string title, string author, string review) NormalizeAndValidate(string title, string author, string? review)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Title is required.");
        if (string.IsNullOrWhiteSpace(author)) throw new DomainException("Author is required.");
        var t = title.Trim();
        var a = author.Trim();
        if (t.Length > MaxTitleLength) throw new DomainException($"Title exceeds {MaxTitleLength} characters.");
        if (a.Length > MaxAuthorLength) throw new DomainException($"Author exceeds {MaxAuthorLength} characters.");
        var r = review ?? string.Empty;
        if (r.Length > MaxReviewLength) throw new DomainException($"Review exceeds {MaxReviewLength} characters.");
        return (t, a, r);
    }

    private static void EnsureOwner(Guid userId)
    {
        if (userId == Guid.Empty) throw new DomainException("UserId is required.");
    }
}
