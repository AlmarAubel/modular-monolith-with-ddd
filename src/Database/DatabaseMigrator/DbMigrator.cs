using DbUp;
using DbUp.ScriptProviders;
using Serilog;

namespace DatabaseMigrator;

public static class DbMigrator
{
    public static bool Migrate(string connectionString, string scriptsPath, ILogger logger)
    {
        if (!Directory.Exists(scriptsPath))
        {
            logger.Information($"Directory {scriptsPath} does not exist");
            return false;
        }

        var serilogUpgradeLog = new SerilogUpgradeLog(logger);

        var upgrader =
            DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsFromFileSystem(scriptsPath, new FileSystemScriptOptions
                {
                    IncludeSubDirectories = true
                })
                .LogTo(serilogUpgradeLog)
                .JournalToSqlTable("app", "MigrationsJournal")
                .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            logger.Information("Migration failed. Message: {Message}", result.Error.Message);
            return false;
        }

        logger.Information("Migration successful");
        return true;
    }
}