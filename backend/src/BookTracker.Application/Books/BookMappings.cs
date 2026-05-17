using BookTracker.Domain.Books;

namespace BookTracker.Application.Books;

internal static class BookMappings
{
    public static BookDto ToDto(this Book b) =>
        new(b.Id, b.Title, b.Author, b.Rating.Value, b.Review, b.ReadAt, b.CreatedAt, b.UpdatedAt);
}
