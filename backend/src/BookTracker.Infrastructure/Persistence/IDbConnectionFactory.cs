using System.Data.Common;

namespace BookTracker.Infrastructure.Persistence;

public interface IDbConnectionFactory
{
    Task<DbConnection> OpenAsync(CancellationToken ct);
}
