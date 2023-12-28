using DbUp;
using DbUp.ScriptProviders;
using Serilog;
using Serilog.Formatting.Compact;

namespace DatabaseMigrator
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var logsPath = "logs\\migration-logs";

            ILogger logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(new CompactJsonFormatter(), logsPath)
                .CreateLogger();

            logger.Information("Logger configured. Starting migration...");

            if (args.Length != 2)
            {
                logger.Error("Invalid arguments. Execution: DatabaseMigrator [connectionString] [pathToScripts].");

                logger.Information("Migration stopped");

                return -1;
            }

            var connectionString = args[0];
            var scriptsPath = args[1];

            var migrationSuccessful = DbMigrator.Migrate(connectionString, scriptsPath, logger);

            return migrationSuccessful ? 0 : -1;
        }
    }
}
