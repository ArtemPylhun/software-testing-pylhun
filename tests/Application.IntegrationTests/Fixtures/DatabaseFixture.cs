using Application.IntegrationTests.Helpers.DataSeeding;
using Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Microsoft.AspNetCore.Mvc.Testing;


namespace Application.IntegrationTests.Fixtures;

public class DatabaseFixture : IAsyncLifetime
{
    public PostgreSqlContainer DbContainer { get; } = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("appointment_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public string ConnectionString => DbContainer.GetConnectionString();

    public async Task InitializeAsync() => await DbContainer.StartAsync();
    
    public async Task DisposeAsync() => await DbContainer.DisposeAsync();
}