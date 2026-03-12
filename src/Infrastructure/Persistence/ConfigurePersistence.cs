using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Infrastructure.Persistence;

public static class ConfigurePersistence
{
    public static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Configure Npgsql data source with dynamic JSON support
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        // 2. Register AppDbContext with PostgreSQL
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                    dataSource,
                    builder => builder.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
                .UseSnakeCaseNamingConvention()  // Converts C# PascalCase to SQL snake_case
                .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning)));

        // 3. Register database initializer for seeding and migrations
        services.AddScoped<AppDbContextInitializer>();

        // 4. Register repositories
        services.AddRepositories();
    }

    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ProviderRepository>();
        services.AddScoped<IProviderRepository>(provider => provider.GetRequiredService<ProviderRepository>());
        services.AddScoped<IProviderQueries>(provider => provider.GetRequiredService<ProviderRepository>());

        services.AddScoped<ServiceRepository>();
        services.AddScoped<IServiceRepository>(provider => provider.GetRequiredService<ServiceRepository>());
        services.AddScoped<IServiceQueries>(provider => provider.GetRequiredService<ServiceRepository>());

        services.AddScoped<AppointmentRepository>();
        services.AddScoped<IAppointmentRepository>(provider => provider.GetRequiredService<AppointmentRepository>());
        services.AddScoped<IAppointmentQueries>(provider => provider.GetRequiredService<AppointmentRepository>());
    }
}