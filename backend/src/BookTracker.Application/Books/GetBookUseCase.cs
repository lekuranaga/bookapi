using BookTracker.Application.Abstractions;
using BookTracker.Application.Common;
using BookTracker.Domain.Books;

namespace BookTracker.Application.Books;

public sealed class GetBookUseCase(IBookRepository books, ICurrentUser user)
{
    public async Task<BookDto> ExecuteAsync(Guid id, CancellationToken ct)
    {
        var book = await books.FindAsync(id, user.Id, ct)
            ?? throw new NotFoundException("Book", id);
        return book.ToDto();
    }
}

public sealed class ListBooksUseCase(IBookRepository books, ICurrentUser user)
{
    public async Task<IReadOnlyList<BookDto>> ExecuteAsync(CancellationToken ct)
    {
        var list = await books.ListAsync(user.Id, ct);
        return list.Select(b => b.ToDto()).ToList();
    }
}
