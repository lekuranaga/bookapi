using BookTracker.Application.Abstractions;
using BookTracker.Application.Abstractions.Repositories;
using BookTracker.Application.Common;

namespace BookTracker.Application.Books;

public sealed class GetBookUseCase
{
    private readonly IBookRepository _books;
    private readonly ICurrentUser _user;

    public GetBookUseCase(IBookRepository books, ICurrentUser user)
    {
        _books = books;
        _user = user;
    }

    public async Task<BookDto> ExecuteAsync(Guid id, CancellationToken ct)
    {
        var book = await _books.FindAsync(id, _user.Id, ct)
            ?? throw new NotFoundException("Book", id);
        return book.ToDto();
    }
}

public sealed class ListBooksUseCase
{
    private readonly IBookRepository _books;
    private readonly ICurrentUser _user;

    public ListBooksUseCase(IBookRepository books, ICurrentUser user)
    {
        _books = books;
        _user = user;
    }

    public async Task<IReadOnlyList<BookDto>> ExecuteAsync(CancellationToken ct)
    {
        var books = await _books.ListAsync(_user.Id, ct);
        return books.Select(b => b.ToDto()).ToList();
    }
}
