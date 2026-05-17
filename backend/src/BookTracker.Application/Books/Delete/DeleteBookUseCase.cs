using BookTracker.Application.Abstractions;
using BookTracker.Application.Common.Exceptions;
using BookTracker.Domain.Books;

namespace BookTracker.Application.Books.Delete;

public sealed class DeleteBookUseCase(IBookRepository books, ICurrentUser user)
{
    public async Task ExecuteAsync(Guid id, CancellationToken ct)
    {
        if (!await books.DeleteAsync(id, user.Id, ct))
            throw new NotFoundException("Book", id);
    }
}
