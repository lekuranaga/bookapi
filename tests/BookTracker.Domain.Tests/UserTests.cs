using BookTracker.Domain.Entities;
using BookTracker.Domain.Exceptions;
using FluentAssertions;

namespace BookTracker.Domain.Tests;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var user = User.Create("john@example.com", "hashedpw");

        user.Id.Should().NotBeEmpty();
        user.Email.Should().Be("john@example.com");
        user.PasswordHash.Should().Be("hashedpw");
    }

    [Fact]
    public void Create_LowercasesEmail()
    {
        var user = User.Create("John@Example.COM", "h");
        user.Email.Should().Be("john@example.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("@x.com")]
    [InlineData("x@")]
    public void Create_WithInvalidEmail_ShouldThrow(string? email)
    {
        var act = () => User.Create(email!, "h");
        act.Should().Throw<DomainException>().WithMessage("*email*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyPasswordHash_ShouldThrow(string? hash)
    {
        var act = () => User.Create("a@b.com", hash!);
        act.Should().Throw<DomainException>().WithMessage("*password*");
    }

    [Fact]
    public void Create_SetsCreatedAtToUtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var user = User.Create("a@b.com", "h");
        user.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
        user.CreatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Hydrate_WithInvalidEmail_ShouldThrow()
    {
        var act = () => User.Hydrate(Guid.NewGuid(), "bad", "h", DateTime.UtcNow);
        act.Should().Throw<DomainException>();
    }
}
