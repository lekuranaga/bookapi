using BookTracker.Domain.Entities;
using BookTracker.Infrastructure.Persistence;
using FluentAssertions;

namespace BookTracker.Infrastructure.Tests;

[Collection(nameof(PostgresCollection))]
public class UserRepositoryTests
{
    private readonly UserRepository _sut;

    public UserRepositoryTests(PostgresFixture fixture)
    {
        _sut = new UserRepository(fixture.ConnectionFactory);
    }

    [Fact]
    public async Task AddThenFindByEmail_RoundTripsUser()
    {
        var user = User.Create($"u{Guid.NewGuid():N}@x.com", "hash");
        await _sut.AddAsync(user, CancellationToken.None);

        var fetched = await _sut.FindByEmailAsync(user.Email, CancellationToken.None);

        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(user.Id);
        fetched.Email.Should().Be(user.Email);
        fetched.PasswordHash.Should().Be("hash");
    }

    [Fact]
    public async Task FindByEmail_Missing_ReturnsNull()
    {
        var result = await _sut.FindByEmailAsync($"missing-{Guid.NewGuid():N}@x.com", CancellationToken.None);
        result.Should().BeNull();
    }
}
