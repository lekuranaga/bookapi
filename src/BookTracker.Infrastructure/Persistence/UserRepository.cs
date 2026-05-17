using BookTracker.Application.Abstractions.Repositories;
using BookTracker.Domain.Entities;
using Npgsql;
using NpgsqlTypes;

namespace BookTracker.Infrastructure.Persistence;

public sealed class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _factory;

    public UserRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<User?> FindByEmailAsync(string email, CancellationToken ct)
    {
        const string sql = "SELECT id, email, password_hash, created_at FROM users WHERE email = @email LIMIT 1;";
        await using var conn = await _factory.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, (NpgsqlConnection)conn);
        cmd.Parameters.Add("email", NpgsqlDbType.Text).Value = email;

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        return Map(reader);
    }

    public async Task<User?> FindByIdAsync(Guid id, CancellationToken ct)
    {
        const string sql = "SELECT id, email, password_hash, created_at FROM users WHERE id = @id LIMIT 1;";
        await using var conn = await _factory.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, (NpgsqlConnection)conn);
        cmd.Parameters.Add("id", NpgsqlDbType.Uuid).Value = id;

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        return Map(reader);
    }

    public async Task AddAsync(User user, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO users (id, email, password_hash, created_at)
            VALUES (@id, @email, @hash, @createdAt);
            """;
        await using var conn = await _factory.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, (NpgsqlConnection)conn);
        cmd.Parameters.Add("id", NpgsqlDbType.Uuid).Value = user.Id;
        cmd.Parameters.Add("email", NpgsqlDbType.Text).Value = user.Email;
        cmd.Parameters.Add("hash", NpgsqlDbType.Text).Value = user.PasswordHash;
        cmd.Parameters.Add("createdAt", NpgsqlDbType.TimestampTz).Value = user.CreatedAt;
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static User Map(NpgsqlDataReader r)
        => User.Hydrate(
            r.GetGuid(0),
            r.GetString(1),
            r.GetString(2),
            r.GetFieldValue<DateTime>(3));
}
