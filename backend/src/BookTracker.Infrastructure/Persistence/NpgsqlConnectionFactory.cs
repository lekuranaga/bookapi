using System.Data.Common;
using Microsoft.Extensions.Options;
using Npgsql;

namespace BookTracker.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";
    public string ConnectionString { get; init; } = string.Empty;
}

public sealed class NpgsqlConnectionFactory(IOptions<DatabaseOptions> options) : IDbConnectionFactory
{
    private readonly NpgsqlDataSource _dataSource = string.IsNullOrWhiteSpace(options.Value.ConnectionString)
        ? throw new InvalidOperationException("Database:ConnectionString is not configured.")
        : NpgsqlDataSource.Create(options.Value.ConnectionString);

    public async Task<DbConnection> OpenAsync(CancellationToken ct)
        => await _dataSource.OpenConnectionAsync(ct);
}
