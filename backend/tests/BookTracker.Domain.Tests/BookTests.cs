using BookTracker.Domain.Books;
using BookTracker.Domain.Common;
using FluentAssertions;

namespace BookTracker.Domain.Tests;

public class BookTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public void Log_WithValidData_ShouldSucceed()
    {
        var book = Book.Log(UserId, "Clean Code", "Robert C. Martin", Rating.Of(5), "great", Today);

        book.Id.Should().NotBeEmpty();
        book.Title.Should().Be("Clean Code");
        book.Author.Should().Be("Robert C. Martin");
        book.Rating.Value.Should().Be(5);
        book.Review.Should().Be("great");
        book.UserId.Should().Be(UserId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Rating_OutOfRange_ShouldThrow(int rating)
    {
        var act = () => Rating.Of(rating);
        act.Should().Throw<DomainException>().WithMessage("*1 and 5*");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void Rating_AtBoundaries_ShouldSucceed(int rating)
    {
        var act = () => Rating.Of(rating);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Log_WithEmptyTitle_ShouldThrow(string? title)
    {
        var act = () => Book.Log(UserId, title!, "a", Rating.Of(3), "r", Today);
        act.Should().Throw<DomainException>().WithMessage("*Title*");
    }

    [Fact]
    public void Log_WithEmptyAuthor_ShouldThrow()
    {
        var act = () => Book.Log(UserId, "t", "", Rating.Of(3), "r", Today);
        act.Should().Throw<DomainException>().WithMessage("*Author*");
    }

    [Fact]
    public void Log_WithEmptyUserId_ShouldThrow()
    {
        var act = () => Book.Log(Guid.Empty, "t", "a", Rating.Of(3), "r", Today);
        act.Should().Throw<DomainException>().WithMessage("*UserId*");
    }

    [Fact]
    public void Revise_WithValidData_ShouldChangeValues()
    {
        var book = Book.Log(UserId, "t", "a", Rating.Of(3), "r", Today);
        book.Revise("t2", "a2", Rating.Of(4), "r2", Today.AddDays(-1));

        book.Title.Should().Be("t2");
        book.Author.Should().Be("a2");
        book.Rating.Value.Should().Be(4);
        book.Review.Should().Be("r2");
    }

    [Fact]
    public void Log_TrimsTitleAndAuthor()
    {
        var book = Book.Log(UserId, "  Clean Code  ", "  Bob  ", Rating.Of(3), null, Today);
        book.Title.Should().Be("Clean Code");
        book.Author.Should().Be("Bob");
    }

    [Fact]
    public void Log_WithNullReview_ShouldCoalesceToEmpty()
    {
        var book = Book.Log(UserId, "t", "a", Rating.Of(3), null, Today);
        book.Review.Should().BeEmpty();
    }

    [Fact]
    public async Task Revise_ShouldAdvanceUpdatedAt()
    {
        var book = Book.Log(UserId, "t", "a", Rating.Of(3), "r", Today);
        var before = book.UpdatedAt;
        await Task.Delay(15);
        book.Revise("t", "a", Rating.Of(4), "r", Today);
        book.UpdatedAt.Should().BeAfter(before);
    }

    [Fact]
    public void Hydrate_WithInvalidRating_ShouldThrow()
    {
        var act = () => Book.Hydrate(Guid.NewGuid(), UserId, "t", "a", 99, "r", Today, DateTime.UtcNow, DateTime.UtcNow);
        act.Should().Throw<DomainException>();
    }
}
