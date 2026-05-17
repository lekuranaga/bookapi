namespace BookTracker.Application.Books.Update;

public sealed record UpdateBookRequest(string Title, string Author, int Rating, string? Review, DateOnly ReadAt);
