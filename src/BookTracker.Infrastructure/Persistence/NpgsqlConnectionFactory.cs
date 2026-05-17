using System.Data.Common;
using Microsoft.Extensions.Options;
using Npgsql;

namespace BookTracker.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";
    public string ConnectionString { get; init; } = string.Empty;
}

public sealed class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly NpgsqlDataSource _dataSource;

    public NpgsqlConnectionFactory(IOptions<DatabaseOptions> options)
    {
        var cs = options.Value.ConnectionString;
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("Database:ConnectionString is not configured.");
        _dataSource = NpgsqlDataSource.Create(cs);
    }

    public async Task<DbConnection> OpenAsync(CancellationToken ct)
        => await _dataSource.OpenConnectionAsync(ct);
}
