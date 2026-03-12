using System.Data.Common;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Respawn.Graph;

namespace Application.IntegrationTests;

public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    private Respawner _respawner = default!;
    private DbConnection _connection = default!;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        // 1. Відкриваємо з'єднання
        _connection = new NpgsqlConnection(Factory.ConnectionString);
        await _connection.OpenAsync();

        // 2. Створюємо Respawner ПІСЛЯ того, як Factory відпрацював MigrateAsync()
        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = new Table[] { "providers", "services", "provider_services" }
        });
    }
    
    protected internal AppDbContext GetDbContext() 
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }
    
    protected async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_connection);
    }
    public async Task DisposeAsync()
    {
        // 3. Скидаємо базу після кожного тесту
        await _respawner.ResetAsync(_connection);
        await _connection.CloseAsync();
    }
}