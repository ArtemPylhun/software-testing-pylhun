using Application;
using Application.Appointments.Commands;
using Application.IntegrationTests.Fixtures;
using Application.IntegrationTests.Helpers.DataSeeding;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Polly;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly DatabaseFixture _dbFixture = new();
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private static bool _isInitialized = false;
    public string ConnectionString => _dbFixture.ConnectionString;

    public async Task InitializeAsync()
    {
        // 1. Спочатку тільки контейнер
        await _dbFixture.InitializeAsync();

        // 2. Гарантуємо виконання ініціалізації бази лише один раз
        await _semaphore.WaitAsync();
        try
        {
            if (!_isInitialized)
            {
                // Використовуємо стратегію спроб для підключення
                using var scope = Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Чекаємо на готовність БД
                await Policy.Handle<Exception>()
                    .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(Math.Pow(2, i)))
                    .ExecuteAsync(async () => await context.Database.MigrateAsync());

                var seeder = new DatabaseSeeder(context);
                await seeder.SeedAsync(providerCount: 50, targetTotalAppointments: 10000);
            
                _isInitialized = true;
            }
        }
        finally { _semaphore.Release(); }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Видаляємо дефолтний DbContext (наприклад, з appsettings чи InMemory)
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Налаштовуємо NpgsqlDataSource
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(_dbFixture.ConnectionString);
            dataSourceBuilder.EnableDynamicJson();
            var dataSource = dataSourceBuilder.Build();

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(dataSource)
                    .UseSnakeCaseNamingConvention());
        }).ConfigureAppConfiguration((_, config) =>
        {
            config
                .AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
        });
    }
    
    public new async Task DisposeAsync() => await _dbFixture.DisposeAsync();
}