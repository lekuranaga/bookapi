using BookTracker.Domain.Common;
using BookTracker.Domain.Users;
using FluentAssertions;

namespace BookTracker.Domain.Tests;

public class UserTests
{
    [Fact]
    public void Register_WithValidData_ShouldSucceed()
    {
        var user = User.Register(Email.Create("john@example.com"), "hashedpw");

        user.Id.Should().NotBeEmpty();
        user.Email.Value.Should().Be("john@example.com");
        user.PasswordHash.Should().Be("hashedpw");
    }

    [Fact]
    public void Email_Create_LowercasesAndTrims()
    {
        var email = Email.Create("  John@Example.COM  ");
        email.Value.Should().Be("john@example.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("@x.com")]
    [InlineData("x@")]
    public void Email_Create_Invalid_ShouldThrow(string? raw)
    {
        var act = () => Email.Create(raw!);
        act.Should().Throw<DomainException>().WithMessage("*email*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_WithEmptyPasswordHash_ShouldThrow(string? hash)
    {
        var act = () => User.Register(Email.Create("a@b.com"), hash!);
        act.Should().Throw<DomainException>().WithMessage("*password*");
    }

    [Fact]
    public void Email_Equality_ByValue()
    {
        var a = Email.Create("x@y.com");
        var b = Email.Create("X@Y.COM");
        a.Should().Be(b);
    }

    [Fact]
    public void Register_SetsCreatedAtToUtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var user = User.Register(Email.Create("a@b.com"), "h");
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
