using Application.IntegrationTests.Fixtures;
using Application.IntegrationTests.Helpers.DataSeeding;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Npgsql;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly DatabaseFixture _dbFixture = new();

    public string ConnectionString => _dbFixture.ConnectionString;
    
    public async Task InitializeAsync()
    {
        // 1. Запускаємо контейнер
        await _dbFixture.InitializeAsync();

        // 2. Створюємо клієнта, щоб примусово запустити ініціалізацію Host (і ConfigureWebHost)
        // Це гарантує, що Services вже доступні і налаштовані
        var client = CreateClient(); 
    
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // 3. Створюємо схему
        await context.Database.EnsureCreatedAsync(); 

        // 4. Запускаємо наш сідер на 10,000 записів
        var seeder = new DatabaseSeeder(context);
        await seeder.SeedAsync(providerCount: 50, targetTotalAppointments: 10000);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
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
        });
    }

    public new async Task DisposeAsync() => await _dbFixture.DisposeAsync();
}