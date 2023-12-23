using DbUp;
using DbUp.ScriptProviders;
using Serilog;

namespace DatabaseMigrator;

public static class DbMigrator
{
    public static bool Migrate(string connectionString, string scriptsPath, ILogger logger, MigrationOptions options = null)
    {
        options ??= new MigrationOptions();

        if (options.CreateDabaseIfNotExists)
        {
            EnsureDatabase.For.SqlDatabase(connectionString);
        }

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
                .JournalToSqlTable("dbo", "MigrationsJournal")
                .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            logger.Information("Migration failed");

            return false;
        }

        logger.Information("Migration successful");

        return true;
    }
}