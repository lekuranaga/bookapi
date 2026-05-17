using BookTracker.Application.Abstractions;
using BookTracker.Domain.Books;

namespace BookTracker.Application.Books;

public sealed class CreateBookUseCase(IBookRepository books, ICurrentUser user)
{
    public async Task<BookDto> ExecuteAsync(CreateBookRequest request, CancellationToken ct)
    {
        var book = Book.Log(user.Id, request.Title, request.Author, Rating.Of(request.Rating), request.Review, request.ReadAt);
        await books.AddAsync(book, ct);
        return book.ToDto();
    }
}
