using BookTracker.Domain.Users;
using BookTracker.Infrastructure.Persistence;
using FluentAssertions;

namespace BookTracker.Infrastructure.Tests;

[Collection(nameof(PostgresCollection))]
public class UserRepositoryTests(PostgresFixture fixture)
{
    private readonly UserRepository _sut = new(fixture.ConnectionFactory);

    [Fact]
    public async Task AddThenFindByEmail_RoundTripsUser()
    {
        var email = Email.Create($"u{Guid.NewGuid():N}@x.com");
        var user = User.Register(email, "hash");
        await _sut.AddAsync(user, CancellationToken.None);

        var fetched = await _sut.FindByEmailAsync(email, CancellationToken.None);

        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(user.Id);
        fetched.Email.Should().Be(email);
        fetched.PasswordHash.Should().Be("hash");
    }

    [Fact]
    public async Task FindByEmail_Missing_ReturnsNull()
    {
        var result = await _sut.FindByEmailAsync(Email.Create($"missing-{Guid.NewGuid():N}@x.com"), CancellationToken.None);
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindById_RoundTripsUser()
    {
        var email = Email.Create($"u{Guid.NewGuid():N}@x.com");
        var user = User.Register(email, "hash");
        await _sut.AddAsync(user, CancellationToken.None);

        var fetched = await _sut.FindByIdAsync(user.Id, CancellationToken.None);

        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task FindById_Missing_ReturnsNull()
    {
        var result = await _sut.FindByIdAsync(Guid.NewGuid(), CancellationToken.None);
        result.Should().BeNull();
    }
}
