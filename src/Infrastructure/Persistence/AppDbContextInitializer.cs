using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContextInitializer
{
    private readonly AppDbContext _context;

    public AppDbContextInitializer(AppDbContext context)
    {
        _context = context;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    /*public async Task SeedAsync()
    {
        try
        {
        }
        catch (Exception ex)
        {
            throw;
        }
    }*/
}