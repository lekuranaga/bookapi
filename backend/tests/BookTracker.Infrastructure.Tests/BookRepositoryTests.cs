using BookTracker.Domain.Books;
using BookTracker.Domain.Users;
using BookTracker.Infrastructure.Persistence;
using FluentAssertions;

namespace BookTracker.Infrastructure.Tests;

[Collection(nameof(PostgresCollection))]
public class BookRepositoryTests(PostgresFixture fixture)
{
    private readonly BookRepository _books = new(fixture.ConnectionFactory);
    private readonly UserRepository _users = new(fixture.ConnectionFactory);

    private async Task<Guid> SeedUserAsync()
    {
        var user = User.Register(Email.Create($"u{Guid.NewGuid():N}@x.com"), "h");
        await _users.AddAsync(user, CancellationToken.None);
        return user.Id;
    }

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    private static Book NewBook(Guid uid, string title = "t", int rating = 3)
        => Book.Log(uid, title, "a", Rating.Of(rating), "r", Today);

    [Fact]
    public async Task Add_then_Find_RoundTripsBook()
    {
        var userId = await SeedUserAsync();
        var book = Book.Log(userId, "t", "a", Rating.Of(5), "great", Today);
        await _books.AddAsync(book, CancellationToken.None);

        var fetched = await _books.FindAsync(book.Id, userId, CancellationToken.None);

        fetched.Should().NotBeNull();
        fetched!.Title.Should().Be("t");
        fetched.Rating.Value.Should().Be(5);
        fetched.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task Find_ForeignUser_ReturnsNull()
    {
        var userA = await SeedUserAsync();
        var userB = await SeedUserAsync();
        var book = NewBook(userA);
        await _books.AddAsync(book, CancellationToken.None);

        var fetched = await _books.FindAsync(book.Id, userB, CancellationToken.None);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task List_ReturnsOnlyOwnedByUser()
    {
        var userA = await SeedUserAsync();
        var userB = await SeedUserAsync();
        await _books.AddAsync(NewBook(userA, "a1", 3), CancellationToken.None);
        await _books.AddAsync(NewBook(userA, "a2", 4), CancellationToken.None);
        await _books.AddAsync(NewBook(userB, "b1", 5), CancellationToken.None);

        var listA = await _books.ListAsync(userA, CancellationToken.None);
        listA.Should().HaveCount(2);
        listA.Should().OnlyContain(b => b.UserId == userA);
    }

    [Fact]
    public async Task Update_PersistsChanges()
    {
        var userId = await SeedUserAsync();
        var book = NewBook(userId);
        await _books.AddAsync(book, CancellationToken.None);

        book.Revise("t2", "a2", Rating.Of(5), "r2", Today);
        await _books.UpdateAsync(book, CancellationToken.None);

        var fetched = await _books.FindAsync(book.Id, userId, CancellationToken.None);
        fetched!.Title.Should().Be("t2");
        fetched.Rating.Value.Should().Be(5);
    }

    [Fact]
    public async Task Delete_ForOwner_ReturnsTrue_AndRemoves()
    {
        var userId = await SeedUserAsync();
        var book = NewBook(userId);
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
        var book = NewBook(userA);
        await _books.AddAsync(book, CancellationToken.None);

        var deleted = await _books.DeleteAsync(book.Id, userB, CancellationToken.None);
        deleted.Should().BeFalse();
        (await _books.FindAsync(book.Id, userA, CancellationToken.None)).Should().NotBeNull();
    }
}
