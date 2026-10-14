using DigitalAgriculturalMarketPlace.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DigitalAgriculturalMarketPlace.Tests;

public sealed class SqlServerTestDatabase : IAsyncLifetime
{
    private readonly string _databaseName =
        $"DigitalAgriculturalMarketplace_Phase2Tests_{Guid.NewGuid():N}";

    public string ConnectionString =>
        Environment.GetEnvironmentVariable("PHASE2_SQLSERVER_TEST_CONNECTION")
        ?? $"Server=(localdb)\\MSSQLLocalDB;Database={_databaseName};Trusted_Connection=True;TrustServerCertificate=True";

    public ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new ApplicationDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
    }
}
