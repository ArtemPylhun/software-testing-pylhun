using Domain.Entities;
using Infrastructure.Persistence;

namespace Application.IntegrationTests.Helpers.DataSeeding;

public class DatabaseSeeder
{
    private readonly AppDbContext _context;
    private readonly Random _random = new();

    public DatabaseSeeder(AppDbContext context) => _context = context;

    public async Task SeedAsync(int providerCount = 50, int serviceCount = 20, int targetTotalAppointments = 10000)
    {
        // 1. Генеруємо базові дані
        var services = new ServiceFaker().Generate(serviceCount);
        var providers = new ProviderFaker().Generate(providerCount);

        // 2. Створюємо зв'язки Many-to-Many (Майстер <-> Послуга)
        foreach (var service in services)
        {
            // Кожна послуга надається 3-7 випадковими майстрами
            var assignedProviders = providers.OrderBy(_ => _random.Next()).Take(_random.Next(3, 8));
            foreach (var p in assignedProviders)
            {
                service.AssignProvider(p);
            }
        }

        await _context.Services.AddRangeAsync(services);
        await _context.Providers.AddRangeAsync(providers);
        await _context.SaveChangesAsync();

        // 3. Генеруємо записи (Appointments)
        var allAppointments = new List<Appointment>();
        int appointmentsPerProvider = targetTotalAppointments / providerCount;

        foreach (var provider in providers)
        {
            // Беремо послуги, які доступні саме цьому провайдеру
            var availableServices = services.Where(s => s.Providers.Any(p => p.Id == provider.Id)).ToList();
            if (!availableServices.Any()) continue;

            for (int i = 0; i < appointmentsPerProvider; i++)
            {
                var service = availableServices[_random.Next(availableServices.Count)];
                
                // Генеруємо випадкову дату (наступні 30 днів) та час (у межах робочих годин)
                var date = DateOnly.FromDateTime(DateTime.Now.AddDays(_random.Next(1, 31)));
                var startTime = new TimeOnly(_random.Next(provider.StartWorkingHours.Hour, provider.EndWorkingHours.Hour - 1), 0);

                try 
                {
                    var appt = TestDataBuilder.BuildAppointment(provider, service, date, startTime);
                    
                    // Використовуємо доменну логіку для перевірки перетинів
                    provider.AddAppointment(appt); 
                    allAppointments.Add(appt);
                }
                catch (InvalidOperationException) 
                {
                    // Якщо трапився перетин — ігноруємо і йдемо далі
                    continue; 
                }
            }
        }

        // 4. Масове збереження
        await _context.Appointments.AddRangeAsync(allAppointments);
        await _context.SaveChangesAsync();
    }
}