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
        // ARRANGE - Явно задаємо бізнес-параметри
        var workStart = new TimeOnly(9, 0);
        var workEnd = new TimeOnly(18, 0);
        var serviceDuration = 60;
        var testDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));
        var bookingTime = new TimeOnly(11, 0);

        await using var db = GetDbContext();
    
        // Створюємо провайдера ЯВНО
        var provider = TestDataBuilder.BuildProvider(workStart, workEnd);
        var service = TestDataBuilder.BuildService(serviceDuration);
        provider.AssignService(service);
    
        db.Providers.Add(provider);
        await db.SaveChangesAsync();

        // Бронюємо слот ЯВНО
        var appointment = Appointment.Create(
            provider, service, "Client", "c@c.com", testDate, bookingTime);
        provider.AddAppointment(appointment);
        await db.SaveChangesAsync();

        // ACT
        var response = await Client.GetAsync($"/api/providers/{provider.Id.Value}/slots?date={testDate:yyyy-MM-dd}");

        // ASSERT
        var slots = await response.Content.ReadFromJsonAsync<List<string>>();
    
        // Тепер ми ТОЧНО знаємо, що 09:00 має бути, а 11:00 - ні.
        slots.Should().Contain("09:00:00"); 
        slots.Should().NotContain("11:00:00");
    }
}