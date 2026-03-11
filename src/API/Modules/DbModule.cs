using Infrastructure.Persistence;

namespace API.Modules;

public static class DbModule
{
    public static async Task InitializeDb(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<AppDbContextInitializer>();
        await initializer.InitializeAsync();
        
        /*var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        if (bool.Parse(config["AllowSeeder"]!))
        {
            await app.SeedAll();
        }*/
    }
}