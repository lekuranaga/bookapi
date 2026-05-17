using BookTracker.Application.Abstractions;
using BookTracker.Application.Abstractions.Repositories;
using BookTracker.Application.Common;

namespace BookTracker.Application.Books;

public sealed class DeleteBookUseCase
{
    private readonly IBookRepository _books;
    private readonly ICurrentUser _user;

    public DeleteBookUseCase(IBookRepository books, ICurrentUser user)
    {
        _books = books;
        _user = user;
    }

    public async Task ExecuteAsync(Guid id, CancellationToken ct)
    {
        var deleted = await _books.DeleteAsync(id, _user.Id, ct);
        if (!deleted)
            throw new NotFoundException("Book", id);
    }
}
