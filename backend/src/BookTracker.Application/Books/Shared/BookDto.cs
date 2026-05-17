namespace BookTracker.Application.Books.Shared;

public sealed record BookDto(
    Guid Id,
    string Title,
    string Author,
    int Rating,
    string Review,
    DateOnly ReadAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);
