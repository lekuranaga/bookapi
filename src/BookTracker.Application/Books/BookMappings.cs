using BookTracker.Domain.Entities;

namespace BookTracker.Application.Books;

internal static class BookMappings
{
    public static BookDto ToDto(this Book b) =>
        new(b.Id, b.Title, b.Author, b.Rating, b.Review, b.ReadAt, b.CreatedAt, b.UpdatedAt);
}
