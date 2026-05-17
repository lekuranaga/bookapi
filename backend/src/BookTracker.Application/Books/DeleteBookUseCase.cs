using BookTracker.Application.Abstractions;
using BookTracker.Application.Common;
using BookTracker.Domain.Books;

namespace BookTracker.Application.Books;

public sealed class DeleteBookUseCase(IBookRepository books, ICurrentUser user)
{
    public async Task ExecuteAsync(Guid id, CancellationToken ct)
    {
        var deleted = await books.DeleteAsync(id, user.Id, ct);
        if (!deleted)
            throw new NotFoundException("Book", id);
    }
}
