using BookTracker.Application.Abstractions;
using BookTracker.Application.Abstractions.Repositories;
using BookTracker.Application.Books;
using BookTracker.Application.Common;
using BookTracker.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace BookTracker.Application.Tests.Books;

public class BookUseCaseTests
{
    private readonly IBookRepository _books = Substitute.For<IBookRepository>();
    private readonly ICurrentUser _user = Substitute.For<ICurrentUser>();
    private readonly Guid _uid = Guid.NewGuid();

    public BookUseCaseTests()
    {
        _user.Id.Returns(_uid);
    }

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public async Task Create_PersistsBookOwnedByCurrentUser()
    {
        var sut = new CreateBookUseCase(_books, _user);
        var dto = await sut.ExecuteAsync(new CreateBookRequest("t", "a", 4, "r", Today), CancellationToken.None);

        dto.Title.Should().Be("t");
        dto.Rating.Should().Be(4);
        await _books.Received(1).AddAsync(Arg.Is<Book>(b => b.UserId == _uid), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_NotFound_ThrowsNotFound()
    {
        _books.FindAsync(Arg.Any<Guid>(), _uid, Arg.Any<CancellationToken>()).Returns((Book?)null);
        var sut = new UpdateBookUseCase(_books, _user);

        var act = () => sut.ExecuteAsync(Guid.NewGuid(), new UpdateBookRequest("t", "a", 3, "r", Today), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Update_FoundForCurrentUser_AppliesChanges()
    {
        var book = Book.Create("old", "auth", 2, "rev", _uid, Today);
        _books.FindAsync(book.Id, _uid, Arg.Any<CancellationToken>()).Returns(book);
        var sut = new UpdateBookUseCase(_books, _user);

        var dto = await sut.ExecuteAsync(book.Id, new UpdateBookRequest("new", "auth2", 5, "rev2", Today), CancellationToken.None);

        dto.Title.Should().Be("new");
        dto.Rating.Should().Be(5);
        await _books.Received(1).UpdateAsync(book, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_ForeignBook_ThrowsNotFound()
    {
        _books.FindAsync(Arg.Any<Guid>(), _uid, Arg.Any<CancellationToken>()).Returns((Book?)null);
        var sut = new UpdateBookUseCase(_books, _user);

        var act = () => sut.ExecuteAsync(Guid.NewGuid(), new UpdateBookRequest("t", "a", 3, "r", Today), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_NotFound_ThrowsNotFound()
    {
        _books.DeleteAsync(Arg.Any<Guid>(), _uid, Arg.Any<CancellationToken>()).Returns(false);
        var sut = new DeleteBookUseCase(_books, _user);

        var act = () => sut.ExecuteAsync(Guid.NewGuid(), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_Found_Succeeds()
    {
        _books.DeleteAsync(Arg.Any<Guid>(), _uid, Arg.Any<CancellationToken>()).Returns(true);
        var sut = new DeleteBookUseCase(_books, _user);

        await sut.ExecuteAsync(Guid.NewGuid(), CancellationToken.None);
    }

    [Fact]
    public async Task Get_PassesCurrentUserId()
    {
        var book = Book.Create("t", "a", 3, "r", _uid, Today);
        _books.FindAsync(book.Id, _uid, Arg.Any<CancellationToken>()).Returns(book);
        var sut = new GetBookUseCase(_books, _user);

        var dto = await sut.ExecuteAsync(book.Id, CancellationToken.None);
        dto.Id.Should().Be(book.Id);
    }

    [Fact]
    public async Task List_ReturnsBooksForCurrentUser()
    {
        var b1 = Book.Create("t1", "a", 3, "r", _uid, Today);
        var b2 = Book.Create("t2", "a", 4, "r", _uid, Today);
        _books.ListAsync(_uid, Arg.Any<CancellationToken>()).Returns(new[] { b1, b2 });

        var sut = new ListBooksUseCase(_books, _user);
        var result = await sut.ExecuteAsync(CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(x => x.Title).Should().Contain(new[] { "t1", "t2" });
    }
}
