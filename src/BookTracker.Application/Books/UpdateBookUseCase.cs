using BookTracker.Application.Abstractions;
using BookTracker.Application.Abstractions.Repositories;
using BookTracker.Application.Common;

namespace BookTracker.Application.Books;

public sealed class UpdateBookUseCase
{
    private readonly IBookRepository _books;
    private readonly ICurrentUser _user;

    public UpdateBookUseCase(IBookRepository books, ICurrentUser user)
    {
        _books = books;
        _user = user;
    }

    public async Task<BookDto> ExecuteAsync(Guid id, UpdateBookRequest request, CancellationToken ct)
    {
        var book = await _books.FindAsync(id, _user.Id, ct)
            ?? throw new NotFoundException("Book", id);

        book.Update(request.Title, request.Author, request.Rating, request.Review, request.ReadAt);
        await _books.UpdateAsync(book, ct);
        return book.ToDto();
    }
}
