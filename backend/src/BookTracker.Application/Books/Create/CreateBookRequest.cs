namespace BookTracker.Application.Books.Create;

public sealed record CreateBookRequest(string Title, string Author, int Rating, string? Review, DateOnly ReadAt);
