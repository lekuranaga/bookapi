using BookTracker.Application.Abstractions;
using BookTracker.Application.Books.Shared;
using BookTracker.Application.Common.Exceptions;
using BookTracker.Domain.Books;

namespace BookTracker.Application.Books.Get;

public sealed class GetBookUseCase(IBookRepository books, ICurrentUser user)
{
    public async Task<BookDto> ExecuteAsync(Guid id, CancellationToken ct)
    {
        var book = await books.FindAsync(id, user.Id, ct)
            ?? throw new NotFoundException("Book", id);
        return book.ToDto();
    }
}
