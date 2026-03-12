using System.Net;
using System.Net.Http.Json;
using Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Application.IntegrationTests.Tests;

public class AppointmentIntegrationTests : IntegrationTestBase
{
    public AppointmentIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task BookAppointment_WithValidData_ShouldReturnSuccess()
    {
        // Arrange: Беремо реального майстра та його послугу з бази
        await using var db = GetDbContext();
        var provider = await db.Providers.Include(p => p.Services).FirstAsync(p => p.Services.Any());
        var service = provider.Services.First();

        var request = new 
        {
            ProviderId = provider.Id.Value,
            ServiceId = service.Id.Value,
            ClientName = "Test Client",
            ClientEmail = "client@test.com",
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), // Запис на 5 днів вперед
            StartTime = provider.StartWorkingHours // На самий початок робочого дня
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/appointments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK); // Або Created (201)
        
        // Перевіряємо, чи реально запис з'явився в базі
        await using var dbCheck = GetDbContext();
        var appointment = await dbCheck.Appointments.FirstOrDefaultAsync(a => a.ClientEmail == request.ClientEmail);
        appointment.Should().NotBeNull();
        appointment!.Status.Should().Be(AppointmentStatus.Booked);
    }

    [Fact]
    public async Task BookAppointment_WithOverlappingTime_ShouldReturnConflict()
    {
        // Arrange
        await using var db = GetDbContext();
        var provider = await db.Providers.Include(p => p.Services).FirstAsync(p => p.Services.Any());
        var service = provider.Services.First();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));
        var startTime = provider.StartWorkingHours;

        var request1 = new 
        {
            ProviderId = provider.Id.Value, ServiceId = service.Id.Value,
            ClientName = "Client 1", ClientEmail = "client1@test.com",
            Date = date, StartTime = startTime
        };

        var request2 = new // Той самий час і майстер
        {
            ProviderId = provider.Id.Value, ServiceId = service.Id.Value,
            ClientName = "Client 2", ClientEmail = "client2@test.com",
            Date = date, StartTime = startTime
        };

        // Act
        await Client.PostAsJsonAsync("/api/appointments", request1); // Перший запис проходить
        var response2 = await Client.PostAsJsonAsync("/api/appointments", request2); // Другий має впасти

        // Assert
        response2.StatusCode.Should().Be(HttpStatusCode.Conflict); // Або Conflict (409) залежно від вашого API
    }

    [Fact]
    public async Task CancelAppointment_LessThanTwoHoursBeforeStart_ShouldReturnBadRequest()
    {
        // 1. ARRANGE
        await using var db = GetDbContext();
    
        // Створюємо провайдера, який працює ЦІЛОДОБОВО, щоб тест не падав вночі
        var provider = TestDataBuilder.BuildProvider(TimeOnly.MinValue, TimeOnly.MaxValue);
        var service = TestDataBuilder.BuildService(30);
        provider.AssignService(service);
    
        db.Providers.Add(provider);
        await db.SaveChangesAsync();

        // Тепер беремо "зараз"
        var now = DateTime.UtcNow;
    
        // ВАЖЛИВО: Якщо зараз 23:30, то AddHours(1) перенесе нас на наступний день.
        // Тому дату теж беремо від результату додавання годин.
        var appointmentDateTime = now.AddHours(1); 
        var date = DateOnly.FromDateTime(appointmentDateTime);
        var startTime = TimeOnly.FromDateTime(appointmentDateTime);

        var appointment = Domain.Entities.Appointment.Create(
            provider, service, "Cancel Client", "cancel@test.com", date, startTime);
    
        provider.AddAppointment(appointment);
        await db.SaveChangesAsync();

        // 2. ACT
        var response = await Client.PatchAsync($"/api/appointments/{appointment.Id.Value}/cancel", null);

        // 3. ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    
        // Додатково можна перевірити повідомлення про помилку, якщо твоє API його повертає
        // var content = await response.Content.ReadAsStringAsync();
        // content.Should().Contain("2 hours");
    }
}