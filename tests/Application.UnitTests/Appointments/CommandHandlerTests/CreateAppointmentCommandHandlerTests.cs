using Application.Appointments.Commands;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.ValueObjects;
using NSubstitute;
using Shouldly;

namespace Application.UnitTests.Appointments.CommandHandlerTests;

public class CreateAppointmentCommandHandlerTests
{
    private readonly IAppointmentQueries _appointmentQueries;
    private readonly IProviderQueries _providerQueries;
    private readonly IServiceQueries _serviceQueries;
    private readonly IAppointmentRepository _repository;
    private readonly CreateAppointmentCommandHandler _sut;

    public CreateAppointmentCommandHandlerTests()
    {
        _appointmentQueries = Substitute.For<IAppointmentQueries>();
        _providerQueries = Substitute.For<IProviderQueries>();
        _serviceQueries = Substitute.For<IServiceQueries>();
        _repository = Substitute.For<IAppointmentRepository>();

        // Порядок має відповідати конструктору Handler: Queries -> Repository
        _sut = new CreateAppointmentCommandHandler(
            _repository,
            _appointmentQueries,
            _providerQueries,
            _serviceQueries);
    }

    // --- Допоміжні методи для створення тестових даних ---

    private Provider CreateProvider(TimeOnly start, TimeOnly end)
    {
        return Provider.Create(
            "Dr. House",
            "Diagnostics",
            "house@hospital.com",
            start,
            end);
    }

    private Service CreateService(int duration)
    {
        return Service.Create("Consultation", duration, 100m, "Desc");
    }

    // Оновлено: прибираємо 'end', бо Appointment.Create сам рахує EndTime
    private Appointment CreateAppointment(Provider provider, Service service, TimeOnly start)
    {
        return Appointment.Create(
            provider,
            service,
            "Patient",
            "p@example.com",
            new DateOnly(2024, 10, 20),
            start);
    }

    // ── Тести ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_NoOverlapsAndWithinHours_ReturnsSuccess()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var date = new DateOnly(2024, 10, 20);
        var startTime = new TimeOnly(10, 0);

        var provider = CreateProvider(new TimeOnly(9, 0), new TimeOnly(18, 0));
        var service = CreateService(60);

        // Повертаємо Option.Some(), щоб уникнути помилок типів
        _providerQueries.GetByIdAsync(Arg.Any<ProviderId>())
            .Returns(Optional.OptionExtensions.Some(provider));

        _serviceQueries.GetByIdAsync(Arg.Any<ServiceId>())
            .Returns(Optional.OptionExtensions.Some(service));

        _appointmentQueries.GetScheduleByProviderAsync(Arg.Any<ProviderId>(), date)
            .Returns(Task.FromResult(new List<Appointment>()));
        
        var command = new CreateAppointmentCommand
        {
            ProviderId = providerId,
            ServiceId = serviceId,
            Date = date,
            StartTime = startTime,
            ClientName = "John",
            ClientEmail = "john@test.com"
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert - result це Either, тому перевіряємо IsRight
        result.IsSuccess.ShouldBeTrue();
        await _repository.Received(1).AddAsync(Arg.Any<Appointment>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(9, 30)] // Перетинає початок (існуючий 10:00-11:00)
    [InlineData(10, 30)] // Перетинає кінець
    [InlineData(10, 15)] // Всередині
    public async Task Handle_OverlapWithExistingAppointment_ReturnsError(int hour, int minute)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var date = new DateOnly(2024, 10, 20);
        var startTime = new TimeOnly(hour, minute);

        var provider = CreateProvider(new TimeOnly(9, 0), new TimeOnly(18, 0));
        var service = CreateService(60);

        _providerQueries.GetByIdAsync(Arg.Any<ProviderId>()).Returns(Optional.OptionExtensions.Some(provider));
        _serviceQueries.GetByIdAsync(Arg.Any<ServiceId>()).Returns(Optional.OptionExtensions.Some(service));

        // Створюємо існуючий запис через метод домену (10:00 - 11:00)
        var existing = CreateAppointment(provider, service, new TimeOnly(10, 0));
        _appointmentQueries.GetScheduleByProviderAsync(Arg.Any<ProviderId>(), date)
            .Returns(new List<Appointment> { existing });

        var command = new CreateAppointmentCommand
        {
            ProviderId = providerId,
            ServiceId = serviceId,
            Date = date,
            StartTime = startTime,
            ClientName = "Test",
            ClientEmail = "test@test.com"
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeTrue();
        result.IfError(err => err.Message.ShouldContain("overlap"));
        await _repository.DidNotReceive().AddAsync(Arg.Any<Appointment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OutsideWorkingHours_ReturnsError()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var startTime = new TimeOnly(19, 0); // Провідер до 18:00

        var provider = CreateProvider(new TimeOnly(9, 0), new TimeOnly(18, 0));
        var service = CreateService(30);

        _providerQueries.GetByIdAsync(Arg.Any<ProviderId>()).Returns(Optional.OptionExtensions.Some(provider));
        _serviceQueries.GetByIdAsync(Arg.Any<ServiceId>()).Returns(Optional.OptionExtensions.Some(service));

        var command = new CreateAppointmentCommand
        {
            ProviderId = providerId,
            ServiceId = Guid.NewGuid(),
            StartTime = startTime,
            Date = new DateOnly(2024, 10, 20),
            ClientName = "Test",
            ClientEmail = "test@test.com"
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeTrue();
        result.IfError(err => err.Message.ShouldContain("working hours"));
    }
}