using BookTracker.Application.Abstractions;
using BookTracker.Application.Books.Shared;
using BookTracker.Application.Common;
using BookTracker.Domain.Books;
using FluentValidation;

namespace BookTracker.Application.Books.Create;

public sealed class CreateBookUseCase(
    IValidator<CreateBookRequest> validator,
    IBookRepository books,
    ICurrentUser user)
{
    public async Task<BookDto> ExecuteAsync(CreateBookRequest request, CancellationToken ct)
    {
        await validator.EnsureValidAsync(request, ct);

        var book = Book.Log(user.Id, request.Title, request.Author, Rating.Of(request.Rating), request.Review, request.ReadAt);
        await books.AddAsync(book, ct);
        return book.ToDto();
    }
}
