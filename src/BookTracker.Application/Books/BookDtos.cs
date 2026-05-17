namespace BookTracker.Application.Books;

public sealed record BookDto(
    Guid Id,
    string Title,
    string Author,
    int Rating,
    string Review,
    DateOnly ReadAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record CreateBookRequest(string Title, string Author, int Rating, string? Review, DateOnly ReadAt);
public sealed record UpdateBookRequest(string Title, string Author, int Rating, string? Review, DateOnly ReadAt);
