using BookTracker.Infrastructure.Persistence;
using Microsoft.Extensions.Options;
using Testcontainers.PostgreSql;

namespace BookTracker.Infrastructure.Tests;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public string ConnectionString => _container.GetConnectionString();
    public IDbConnectionFactory ConnectionFactory { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        var opts = Options.Create(new DatabaseOptions { ConnectionString = ConnectionString });
        ConnectionFactory = new NpgsqlConnectionFactory(opts);
        new DatabaseMigrator(opts).Run();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}

[CollectionDefinition(nameof(PostgresCollection))]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture> { }
