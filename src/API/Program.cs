var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Use(async (context, next) =>
{
    try { await next(); }
    catch (Exception ex) 
    {
        // Виведе помилку прямо в консоль, де біжать тести
        Console.WriteLine($"!!! CAUGHT EXCEPTION: {ex.GetType().Name} - {ex.Message}");
        Console.WriteLine(ex.StackTrace);
        throw; // Продовжуємо викидати, щоб не маскувати проблему
    }
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program{}