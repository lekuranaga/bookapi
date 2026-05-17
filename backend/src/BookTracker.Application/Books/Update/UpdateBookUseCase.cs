using BookTracker.Application.Abstractions;
using BookTracker.Application.Books.Shared;
using BookTracker.Application.Common;
using BookTracker.Application.Common.Exceptions;
using BookTracker.Domain.Books;
using FluentValidation;

namespace BookTracker.Application.Books.Update;

public sealed class UpdateBookUseCase(
    IValidator<UpdateBookRequest> validator,
    IBookRepository books,
    ICurrentUser user)
{
    public async Task<BookDto> ExecuteAsync(Guid id, UpdateBookRequest request, CancellationToken ct)
    {
        await validator.EnsureValidAsync(request, ct);

        var book = await books.FindAsync(id, user.Id, ct)
            ?? throw new NotFoundException("Book", id);

        book.Revise(request.Title, request.Author, Rating.Of(request.Rating), request.Review, request.ReadAt);
        await books.UpdateAsync(book, ct);
        return book.ToDto();
    }
}
