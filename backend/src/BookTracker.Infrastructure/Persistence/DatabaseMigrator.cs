using DbUp;
using DbUp.Engine;
using Microsoft.Extensions.Options;

namespace BookTracker.Infrastructure.Persistence;

public sealed class DatabaseMigrator(IOptions<DatabaseOptions> options)
{
    private readonly DatabaseOptions _options = options.Value;

    public DatabaseUpgradeResult Run()
    {
        EnsureDatabase.For.PostgresqlDatabase(_options.ConnectionString);

        var upgrader = DeployChanges.To
            .PostgresqlDatabase(_options.ConnectionString)
            .WithScriptsEmbeddedInAssembly(typeof(DatabaseMigrator).Assembly)
            .WithTransactionPerScript()
            .WithVariablesDisabled()
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();
        if (!result.Successful)
            throw new InvalidOperationException("Database migration failed.", result.Error);
        return result;
    }
}
