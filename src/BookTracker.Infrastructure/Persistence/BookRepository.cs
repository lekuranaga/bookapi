using BookTracker.Application.Abstractions.Repositories;
using BookTracker.Domain.Entities;
using Npgsql;
using NpgsqlTypes;

namespace BookTracker.Infrastructure.Persistence;

public sealed class BookRepository : IBookRepository
{
    private const string Columns = "id, user_id, title, author, rating, review, read_at, created_at, updated_at";

    private readonly IDbConnectionFactory _factory;

    public BookRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<Book?> FindAsync(Guid id, Guid userId, CancellationToken ct)
    {
        const string sql = $"SELECT {Columns} FROM books WHERE id = @id AND user_id = @userId LIMIT 1;";
        await using var conn = await _factory.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, (NpgsqlConnection)conn);
        cmd.Parameters.Add("id", NpgsqlDbType.Uuid).Value = id;
        cmd.Parameters.Add("userId", NpgsqlDbType.Uuid).Value = userId;

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task<IReadOnlyList<Book>> ListAsync(Guid userId, CancellationToken ct)
    {
        const string sql = $"SELECT {Columns} FROM books WHERE user_id = @userId ORDER BY read_at DESC, created_at DESC;";
        await using var conn = await _factory.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, (NpgsqlConnection)conn);
        cmd.Parameters.Add("userId", NpgsqlDbType.Uuid).Value = userId;

        var list = new List<Book>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            list.Add(Map(reader));
        return list;
    }

    public async Task AddAsync(Book book, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO books (id, user_id, title, author, rating, review, read_at, created_at, updated_at)
            VALUES (@id, @userId, @title, @author, @rating, @review, @readAt, @createdAt, @updatedAt);
            """;
        await using var conn = await _factory.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, (NpgsqlConnection)conn);
        Bind(cmd, book);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task UpdateAsync(Book book, CancellationToken ct)
    {
        const string sql = """
            UPDATE books
            SET title = @title, author = @author, rating = @rating, review = @review,
                read_at = @readAt, updated_at = @updatedAt
            WHERE id = @id AND user_id = @userId;
            """;
        await using var conn = await _factory.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, (NpgsqlConnection)conn);
        Bind(cmd, book);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct)
    {
        const string sql = "DELETE FROM books WHERE id = @id AND user_id = @userId;";
        await using var conn = await _factory.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, (NpgsqlConnection)conn);
        cmd.Parameters.Add("id", NpgsqlDbType.Uuid).Value = id;
        cmd.Parameters.Add("userId", NpgsqlDbType.Uuid).Value = userId;
        var affected = await cmd.ExecuteNonQueryAsync(ct);
        return affected > 0;
    }

    private static void Bind(NpgsqlCommand cmd, Book b)
    {
        cmd.Parameters.Add("id", NpgsqlDbType.Uuid).Value = b.Id;
        cmd.Parameters.Add("userId", NpgsqlDbType.Uuid).Value = b.UserId;
        cmd.Parameters.Add("title", NpgsqlDbType.Text).Value = b.Title;
        cmd.Parameters.Add("author", NpgsqlDbType.Text).Value = b.Author;
        cmd.Parameters.Add("rating", NpgsqlDbType.Integer).Value = b.Rating;
        cmd.Parameters.Add("review", NpgsqlDbType.Text).Value = b.Review;
        cmd.Parameters.Add("readAt", NpgsqlDbType.Date).Value = b.ReadAt;
        cmd.Parameters.Add("createdAt", NpgsqlDbType.TimestampTz).Value = b.CreatedAt;
        cmd.Parameters.Add("updatedAt", NpgsqlDbType.TimestampTz).Value = b.UpdatedAt;
    }

    private static Book Map(NpgsqlDataReader r)
        => Book.Hydrate(
            id: r.GetGuid(0),
            userId: r.GetGuid(1),
            title: r.GetString(2),
            author: r.GetString(3),
            rating: r.GetInt32(4),
            review: r.GetString(5),
            readAt: r.GetFieldValue<DateOnly>(6),
            createdAt: r.GetFieldValue<DateTime>(7),
            updatedAt: r.GetFieldValue<DateTime>(8));
}
