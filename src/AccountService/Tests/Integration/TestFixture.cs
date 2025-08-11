using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using AccountService.Data;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using Xunit;

namespace AccountService.Tests.Integration;

public class TestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("testdb")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;
    public HttpClient Client = null!;
    public string ConnectionString = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        ConnectionString = _dbContainer.GetConnectionString();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<DbContextOptions<AccountServiceContext>>();
                    services.AddDbContext<AccountServiceContext>(options =>
                        options.UseNpgsql(ConnectionString));
                });
            });

        Client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AccountServiceContext>();
        await db.Database.MigrateAsync();
    }

    public async Task ResetDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AccountServiceContext>();

        await db.Database.ExecuteSqlRawAsync("SET session_replication_role = 'replica';");

        await db.Database.ExecuteSqlRawAsync("DELETE FROM \"Transactions\";");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM \"Accounts\";");

        await db.Database.ExecuteSqlRawAsync("SET session_replication_role = 'origin';");
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _factory.DisposeAsync();
        Client.Dispose();
    }
}