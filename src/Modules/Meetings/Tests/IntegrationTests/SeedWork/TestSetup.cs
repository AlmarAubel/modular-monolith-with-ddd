using System.Diagnostics;
using DatabaseMigrator;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using Serilog;
using Testcontainers.SqlEdge;

// Note A SetUpFixture in a namespace will apply tests to that exact namespace only, and not to any namespaces below it.
namespace CompanyName.MyMeetings.Modules.Meetings.IntegrationTests;

[SetUpFixture]
public class TestSetup
{
    private SqlEdgeContainer _sqlEdgeContainer;
    private ILogger _logger;

    public static string ConnectionString { get; private set; }

    public TestSetup()
    {
        _sqlEdgeContainer = new SqlEdgeBuilder()
            .WithImage("mcr.microsoft.com/azure-sql-edge:1.0.7")
            .Build();

        _logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] [{Module}] [{Context}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
        await _sqlEdgeContainer.StartAsync();
        ConnectionString = _sqlEdgeContainer.GetConnectionString();

        var solutionRoot = FindSolutionRoot();

        // CreateDatabase(solutionRoot);
        var result = DbMigrator.Migrate(ConnectionString, $"{solutionRoot}/Database/CompanyName.MyMeetings.Database/Scripts/Migrations", _logger, new MigrationOptions { CreateDabaseIfNotExists = true });
        if (!result)
        {
            throw new ApplicationException("Database migration failed");
        }
    }

    [OneTimeTearDown]
    public Task RunAfterAnyTests() => _sqlEdgeContainer?.DisposeAsync().AsTask();

    private string FindSolutionRoot()
    {
        // todo find a nice way to get the solution root dir
        var currentDirectory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (currentDirectory != null && !File.Exists(Path.Combine(currentDirectory.FullName, "CompanyName.MyMeetings.sln")))
        {
            currentDirectory = currentDirectory.Parent;
        }

        var solutionRoot = currentDirectory?.FullName;
        return solutionRoot;
    }
}