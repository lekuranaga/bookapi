using BookTracker.Application.Abstractions;
using BookTracker.Application.Books.Shared;
using BookTracker.Domain.Books;

namespace BookTracker.Application.Books.List;

public sealed class ListBooksUseCase(IBookRepository books, ICurrentUser user)
{
    public async Task<IReadOnlyList<BookDto>> ExecuteAsync(CancellationToken ct)
    {
        var list = await books.ListAsync(user.Id, ct);
        return list.Select(b => b.ToDto()).ToList();
    }
}
