using BookTracker.Domain.Entities;
using BookTracker.Infrastructure.Persistence;
using FluentAssertions;

namespace BookTracker.Infrastructure.Tests;

[Collection(nameof(PostgresCollection))]
public class BookRepositoryTests
{
    private readonly BookRepository _books;
    private readonly UserRepository _users;

    public BookRepositoryTests(PostgresFixture fixture)
    {
        _books = new BookRepository(fixture.ConnectionFactory);
        _users = new UserRepository(fixture.ConnectionFactory);
    }

    private async Task<Guid> SeedUserAsync()
    {
        var user = User.Create($"u{Guid.NewGuid():N}@x.com", "h");
        await _users.AddAsync(user, CancellationToken.None);
        return user.Id;
    }

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public async Task Add_then_Find_RoundTripsBook()
    {
        var userId = await SeedUserAsync();
        var book = Book.Create("t", "a", 5, "great", userId, Today);
        await _books.AddAsync(book, CancellationToken.None);

        var fetched = await _books.FindAsync(book.Id, userId, CancellationToken.None);

        fetched.Should().NotBeNull();
        fetched!.Title.Should().Be("t");
        fetched.Rating.Should().Be(5);
        fetched.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task Find_ForeignUser_ReturnsNull()
    {
        var userA = await SeedUserAsync();
        var userB = await SeedUserAsync();
        var book = Book.Create("t", "a", 3, "r", userA, Today);
        await _books.AddAsync(book, CancellationToken.None);

        var fetched = await _books.FindAsync(book.Id, userB, CancellationToken.None);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task List_ReturnsOnlyOwnedByUser()
    {
        var userA = await SeedUserAsync();
        var userB = await SeedUserAsync();
        await _books.AddAsync(Book.Create("a1", "a", 3, "r", userA, Today), CancellationToken.None);
        await _books.AddAsync(Book.Create("a2", "a", 4, "r", userA, Today), CancellationToken.None);
        await _books.AddAsync(Book.Create("b1", "a", 5, "r", userB, Today), CancellationToken.None);

        var listA = await _books.ListAsync(userA, CancellationToken.None);
        listA.Should().HaveCount(2);
        listA.Should().OnlyContain(b => b.UserId == userA);
    }

    [Fact]
    public async Task Update_PersistsChanges()
    {
        var userId = await SeedUserAsync();
        var book = Book.Create("t", "a", 3, "r", userId, Today);
        await _books.AddAsync(book, CancellationToken.None);

        book.Update("t2", "a2", 5, "r2", Today);
        await _books.UpdateAsync(book, CancellationToken.None);

        var fetched = await _books.FindAsync(book.Id, userId, CancellationToken.None);
        fetched!.Title.Should().Be("t2");
        fetched.Rating.Should().Be(5);
    }

    [Fact]
    public async Task Delete_ForOwner_ReturnsTrue_AndRemoves()
    {
        var userId = await SeedUserAsync();
        var book = Book.Create("t", "a", 3, "r", userId, Today);
        await _books.AddAsync(book, CancellationToken.None);

        var deleted = await _books.DeleteAsync(book.Id, userId, CancellationToken.None);
        deleted.Should().BeTrue();
        (await _books.FindAsync(book.Id, userId, CancellationToken.None)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_ForForeignUser_ReturnsFalse_AndKeeps()
    {
        var userA = await SeedUserAsync();
        var userB = await SeedUserAsync();
        var book = Book.Create("t", "a", 3, "r", userA, Today);
        await _books.AddAsync(book, CancellationToken.None);

        var deleted = await _books.DeleteAsync(book.Id, userB, CancellationToken.None);
        deleted.Should().BeFalse();
        (await _books.FindAsync(book.Id, userA, CancellationToken.None)).Should().NotBeNull();
    }
}
