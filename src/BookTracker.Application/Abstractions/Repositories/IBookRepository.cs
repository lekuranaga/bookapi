using BookTracker.Domain.Entities;

namespace BookTracker.Application.Abstractions.Repositories;

public interface IBookRepository
{
    Task<Book?> FindAsync(Guid id, Guid userId, CancellationToken ct);
    Task<IReadOnlyList<Book>> ListAsync(Guid userId, CancellationToken ct);
    Task AddAsync(Book book, CancellationToken ct);
    Task UpdateAsync(Book book, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct);
}
