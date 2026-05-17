using BookTracker.Application.Abstractions;
using BookTracker.Application.Common;
using BookTracker.Domain.Books;

namespace BookTracker.Application.Books;

public sealed class UpdateBookUseCase(IBookRepository books, ICurrentUser user)
{
    public async Task<BookDto> ExecuteAsync(Guid id, UpdateBookRequest request, CancellationToken ct)
    {
        var book = await books.FindAsync(id, user.Id, ct)
            ?? throw new NotFoundException("Book", id);

        book.Revise(request.Title, request.Author, Rating.Of(request.Rating), request.Review, request.ReadAt);
        await books.UpdateAsync(book, ct);
        return book.ToDto();
    }
}
