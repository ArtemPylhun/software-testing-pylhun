using DotNet.Testcontainers.Builders;
using Testcontainers.PostgreSql;


namespace Application.IntegrationTests.Fixtures;

public class DatabaseFixture : IAsyncLifetime
{
    public PostgreSqlContainer DbContainer { get; } = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("appointment_test_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public string ConnectionString => DbContainer.GetConnectionString();

    public async Task InitializeAsync() => await DbContainer.StartAsync();
    
    public async Task DisposeAsync() => await DbContainer.DisposeAsync();
}