using BookTracker.Domain.Entities;
using BookTracker.Domain.Exceptions;
using FluentAssertions;

namespace BookTracker.Domain.Tests;

public class BookTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var book = Book.Create("Clean Code", "Robert C. Martin", rating: 5, review: "great", userId: UserId, readAt: DateOnly.FromDateTime(DateTime.UtcNow));

        book.Id.Should().NotBeEmpty();
        book.Title.Should().Be("Clean Code");
        book.Author.Should().Be("Robert C. Martin");
        book.Rating.Should().Be(5);
        book.Review.Should().Be("great");
        book.UserId.Should().Be(UserId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Create_WithRatingOutOfRange_ShouldThrow(int rating)
    {
        var act = () => Book.Create("t", "a", rating, "r", UserId, DateOnly.FromDateTime(DateTime.UtcNow));
        act.Should().Throw<DomainException>().WithMessage("*1 and 5*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyTitle_ShouldThrow(string? title)
    {
        var act = () => Book.Create(title!, "a", 3, "r", UserId, DateOnly.FromDateTime(DateTime.UtcNow));
        act.Should().Throw<DomainException>().WithMessage("*Title*");
    }

    [Fact]
    public void Create_WithEmptyAuthor_ShouldThrow()
    {
        var act = () => Book.Create("t", "", 3, "r", UserId, DateOnly.FromDateTime(DateTime.UtcNow));
        act.Should().Throw<DomainException>().WithMessage("*Author*");
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldThrow()
    {
        var act = () => Book.Create("t", "a", 3, "r", Guid.Empty, DateOnly.FromDateTime(DateTime.UtcNow));
        act.Should().Throw<DomainException>().WithMessage("*UserId*");
    }

    [Fact]
    public void Update_WithValidData_ShouldChangeValues()
    {
        var book = Book.Create("t", "a", 3, "r", UserId, DateOnly.FromDateTime(DateTime.UtcNow));

        book.Update("t2", "a2", 4, "r2", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));

        book.Title.Should().Be("t2");
        book.Author.Should().Be("a2");
        book.Rating.Should().Be(4);
        book.Review.Should().Be("r2");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void Create_WithBoundaryRatings_ShouldSucceed(int rating)
    {
        var act = () => Book.Create("t", "a", rating, "r", UserId, DateOnly.FromDateTime(DateTime.UtcNow));
        act.Should().NotThrow();
    }

    [Fact]
    public void Create_TrimsTitleAndAuthor()
    {
        var book = Book.Create("  Clean Code  ", "  Bob  ", 3, null, UserId, DateOnly.FromDateTime(DateTime.UtcNow));
        book.Title.Should().Be("Clean Code");
        book.Author.Should().Be("Bob");
    }

    [Fact]
    public void Create_WithNullReview_ShouldCoalesceToEmpty()
    {
        var book = Book.Create("t", "a", 3, null, UserId, DateOnly.FromDateTime(DateTime.UtcNow));
        book.Review.Should().BeEmpty();
    }

    [Fact]
    public void Update_WithInvalidRating_ShouldThrow()
    {
        var book = Book.Create("t", "a", 3, "r", UserId, DateOnly.FromDateTime(DateTime.UtcNow));
        var act = () => book.Update("t", "a", 7, "r", DateOnly.FromDateTime(DateTime.UtcNow));
        act.Should().Throw<DomainException>().WithMessage("*1 and 5*");
    }

    [Fact]
    public async Task Update_ShouldAdvanceUpdatedAt()
    {
        var book = Book.Create("t", "a", 3, "r", UserId, DateOnly.FromDateTime(DateTime.UtcNow));
        var before = book.UpdatedAt;
        await Task.Delay(15);
        book.Update("t", "a", 4, "r", DateOnly.FromDateTime(DateTime.UtcNow));
        book.UpdatedAt.Should().BeAfter(before);
    }

    [Fact]
    public void Hydrate_WithInvalidRating_ShouldThrow()
    {
        var act = () => Book.Hydrate(Guid.NewGuid(), "t", "a", 99, "r", DateOnly.FromDateTime(DateTime.UtcNow), UserId, DateTime.UtcNow, DateTime.UtcNow);
        act.Should().Throw<DomainException>();
    }
}
