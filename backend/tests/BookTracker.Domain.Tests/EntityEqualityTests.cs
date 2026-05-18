using BookTracker.Domain.Books;
using BookTracker.Domain.Common;
using FluentAssertions;

namespace BookTracker.Domain.Tests;

public class EntityEqualityTests
{
    [Fact]
    public void Entities_WithSameId_AreEqual()
    {
        var id = Guid.NewGuid();
        var uid = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var now = DateTime.UtcNow;

        var a = Book.Hydrate(id, uid, "A", "A", 3, "", today, now, now);
        var b = Book.Hydrate(id, uid, "B", "B", 4, "", today, now, now);

        a.Should().Be(b);
        (a == b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Entities_WithDifferentIds_AreNotEqual()
    {
        var uid = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var a = Book.Log(uid, "A", "A", Rating.Of(3), "", today);
        var b = Book.Log(uid, "A", "A", Rating.Of(3), "", today);

        a.Should().NotBe(b);
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void Entity_NotEqualToNull()
    {
        var book = Book.Log(Guid.NewGuid(), "A", "A", Rating.Of(3), "", DateOnly.FromDateTime(DateTime.UtcNow));
        book.Equals(null!).Should().BeFalse();
    }

    [Fact]
    public void Rating_Equality_ByValue()
    {
        var a = Rating.Of(3);
        var b = Rating.Of(3);
        var c = Rating.Of(4);

        a.Should().Be(b);
        (a == b).Should().BeTrue();
        a.Should().NotBe(c);
        (a != c).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}
