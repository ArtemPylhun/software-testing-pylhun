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
    // Arrange
    await using var db = GetDbContext();
    
    // Використовуємо твій TestDataBuilder, як у 3-му тесті
    var provider = TestDataBuilder.BuildProvider(TimeOnly.MinValue, TimeOnly.MaxValue);
    var service = TestDataBuilder.BuildService(30);
    provider.AssignService(service);
    
    db.Providers.Add(provider);
    await db.SaveChangesAsync();

    var request = new 
    {
        ProviderId = provider.Id.Value,
        ServiceId = service.Id.Value,
        ClientName = "Test Client",
        ClientEmail = "client@test.com",
        Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
        StartTime = new TimeOnly(10, 0) // Стабільний час
    };

    // Act
    var response = await Client.PostAsJsonAsync("/api/appointments", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    
    await using var dbCheck = GetDbContext();
    var appointment = await dbCheck.Appointments.FirstOrDefaultAsync(a => a.ClientEmail == request.ClientEmail);
    appointment.Should().NotBeNull();
}

[Fact]
public async Task BookAppointment_WithOverlappingTime_ShouldReturnConflict()
{
    // Arrange
    await using var db = GetDbContext();
    
    var provider = TestDataBuilder.BuildProvider(TimeOnly.MinValue, TimeOnly.MaxValue);
    var service = TestDataBuilder.BuildService(30);
    provider.AssignService(service);
    
    db.Providers.Add(provider);
    await db.SaveChangesAsync();

    var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));
    var startTime = new TimeOnly(10, 0);

    var request1 = new 
    {
        ProviderId = provider.Id.Value, ServiceId = service.Id.Value,
        ClientName = "Client 1", ClientEmail = "client1@test.com",
        Date = date, StartTime = startTime
    };

    var request2 = new 
    {
        ProviderId = provider.Id.Value, ServiceId = service.Id.Value,
        ClientName = "Client 2", ClientEmail = "client2@test.com",
        Date = date, StartTime = startTime
    };

    // Act
    await Client.PostAsJsonAsync("/api/appointments", request1);
    var response2 = await Client.PostAsJsonAsync("/api/appointments", request2);

    // Assert
    response2.StatusCode.Should().Be(HttpStatusCode.Conflict);
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