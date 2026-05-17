using BookTracker.Application.Abstractions;
using BookTracker.Application.Abstractions.Repositories;
using BookTracker.Domain.Entities;

namespace BookTracker.Application.Books;

public sealed class CreateBookUseCase
{
    private readonly IBookRepository _books;
    private readonly ICurrentUser _user;

    public CreateBookUseCase(IBookRepository books, ICurrentUser user)
    {
        _books = books;
        _user = user;
    }

    public async Task<BookDto> ExecuteAsync(CreateBookRequest request, CancellationToken ct)
    {
        var book = Book.Create(request.Title, request.Author, request.Rating, request.Review, _user.Id, request.ReadAt);
        await _books.AddAsync(book, ct);
        return book.ToDto();
    }
}
