using System.Net;
using System.Net.Http.Json;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Application.IntegrationTests.Tests;

public class ProviderSlotsIntegrationTests : IntegrationTestBase
{
    public ProviderSlotsIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAvailableSlots_ShouldReturnWorkingHours_MinusBookedAppointments()
    {
        // ==========================================
        // 1. ARRANGE (Підготовка передбачуваних даних)
        // ==========================================
        await using var db = GetDbContext();
        
        // Беремо провайдера та його послугу (вони залишилися після сідінгу)
        var provider = await db.Providers.Include(p => p.Services).FirstAsync(p => p.Services.Any());
        var service = provider.Services.First();
        
        // Визначаємо дату для тесту (наприклад, завтра)
        var testDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        
        // Для простоти тесту знайдемо час усередині його робочого дня.
        // Наприклад, візьмемо час через 2 години після початку роботи.
        var appointmentStartTime = provider.StartWorkingHours.AddHours(2);
        
        // Створюємо запис безпосередньо в базі
        var appointment = Appointment.Create(
            provider, 
            service, 
            "Slot Tester", 
            "slot@tester.com", 
            testDate, 
            appointmentStartTime);
            
        provider.AddAppointment(appointment);
        await db.SaveChangesAsync();

        // ==========================================
        // 2. ACT (Виконуємо запит до нашого API)
        // ==========================================
        // Запитуємо слоти на цю конкретну дату
        // Маршрут згідно з ТЗ: GET /api/providers/{id}/slots?date={date}
        // Формат дати: yyyy-MM-dd
        var dateString = testDate.ToString("yyyy-MM-dd");
        var response = await Client.GetAsync($"/api/providers/{provider.Id.Value}/slots?date={dateString}");

        // ==========================================
        // 3. ASSERT (Перевірка результату)
        // ==========================================
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Припускаємо, що API повертає масив рядків з часом (наприклад, ["09:00", "09:30", ...])
        // Підлаштуй під свою реальну DTO, яку повертає контролер
        var availableSlots = await response.Content.ReadFromJsonAsync<List<string>>();

        availableSlots.Should().NotBeNull();
        
        // Головна бізнес-логіка:
        // 1. Слоти не повинні містити часу нашого запису (appointmentStartTime)
        var bookedTimeString = appointmentStartTime.ToString("HH:mm");
        availableSlots.Should().NotContain(bookedTimeString);

        // 2. Слоти ПОВИННІ містити вільний час (наприклад, самий початок робочого дня)
        var freeTimeString = provider.StartWorkingHours.ToString("HH:mm");
        
        // Робимо перевірку лише якщо початок робочого дня не збігається із заброньованим часом
        if (appointmentStartTime != provider.StartWorkingHours)
        {
            availableSlots.Should().Contain(freeTimeString);
        }
    }
}