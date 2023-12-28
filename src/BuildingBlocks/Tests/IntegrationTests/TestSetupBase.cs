using DatabaseMigrator;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using Serilog;
using Testcontainers.SqlEdge;

namespace CompanyName.MyMeetings.BuildingBlocks.IntegrationTests;

public class TestSetupBase
{
    private SqlEdgeContainer _sqlEdgeContainer;

    public static string ConnectionString { get; private set; }

    public static ILogger Logger { get; private set; }

    public TestSetupBase()
    {
        const string connectionStringEnvironmentVariable =
            "ASPNETCORE_MyMeetings_IntegrationTests_ConnectionString";
        ConnectionString = EnvironmentVariablesProvider.GetVariable(connectionStringEnvironmentVariable);

        if (string.IsNullOrEmpty(ConnectionString))
        {
            _sqlEdgeContainer = new SqlEdgeBuilder()
                .WithImage("mcr.microsoft.com/azure-sql-edge:latest")
                .Build();
        }

        Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] [{Module}] [{Context}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {
        if (_sqlEdgeContainer != null)
        {
            await _sqlEdgeContainer.StartAsync();
            ConnectionString = _sqlEdgeContainer.GetConnectionString();
        }

        var solutionRoot = FindSolutionRoot();
        await CreateAppSchema();

        var scriptsPath = $"{solutionRoot}/Database/CompanyName.MyMeetings.Database/Scripts/Migrations";
        var result = DbMigrator.Migrate(ConnectionString, scriptsPath, Logger);

        if (!result)
        {
            throw new ApplicationException("Database migration failed");
        }
    }

    [OneTimeTearDown]
    public Task RunAfterAnyTests() => _sqlEdgeContainer?.DisposeAsync().AsTask();

    private async Task CreateAppSchema()
    {
        var sqlScript = "CREATE SCHEMA app AUTHORIZATION dbo";

        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(sqlScript, connection);
        await command.ExecuteNonQueryAsync();
    }

    private string FindSolutionRoot()
    {
        // todo find a better way to get the solution root dir
        var currentDirectory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (currentDirectory != null && !File.Exists(Path.Combine(currentDirectory.FullName, "CompanyName.MyMeetings.sln")))
        {
            currentDirectory = currentDirectory.Parent;
        }

        var solutionRoot = currentDirectory?.FullName;
        return solutionRoot;
    }
}