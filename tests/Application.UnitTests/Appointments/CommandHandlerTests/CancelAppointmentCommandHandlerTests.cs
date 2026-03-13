using System.Runtime.CompilerServices;
using Application.Appointments.Commands;
using Application.Appointments.Exceptions;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.ValueObjects;
using NSubstitute;
using Shouldly;

namespace Application.UnitTests.Appointments.CommandHandlerTests;

public class CancelAppointmentCommandHandlerTests
{
    private readonly IAppointmentRepository _repository;
    private readonly IAppointmentQueries _queries;
    private readonly CancelAppointmentCommandHandler _sut;

    public CancelAppointmentCommandHandlerTests()
    {
        _repository = Substitute.For<IAppointmentRepository>();
        _queries = Substitute.For<IAppointmentQueries>();

        _sut = new CancelAppointmentCommandHandler(_repository, _queries);
    }

    private Appointment CreateValidAppointment(Guid id)
    {
        var provider = Provider.Create("Dr. House", "Diag", "h@h.com", new TimeOnly(9, 0), new TimeOnly(18, 0));
        var service = Service.Create("Consultation", 60, 100m, "Desc");
        
        // Створюємо чесний запис (він скоріш за все отримає статус Scheduled за замовчуванням)
        var appointment = Appointment.Create(
            provider, 
            service, 
            "Test Client", 
            "test@test.com", 
            new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day).AddDays(1), 
            new TimeOnly(10, 0));

        // І вже після створення підміняємо йому Id через рефлексію, бо це потрібно для моків
        var idProperty = typeof(Appointment).GetProperty(nameof(Appointment.Id));
        idProperty?.SetValue(appointment, new AppointmentId(id));

        return appointment;
    }

    [Fact]
    public async Task Handle_WhenAppointmentExists_CancelsAndUpdate()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var command = new CancelAppointmentCommand { AppointmentId = appointmentId };
        var appointment = CreateValidAppointment(appointmentId);

        _queries.GetByIdAsync(Arg.Any<AppointmentId>(), Arg.Any<CancellationToken>())
            .Returns(Optional.OptionExtensions.Some(appointment));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _repository.Received(1).UpdateAsync(appointment, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAppointmentNotFound_ReturnsError()
    {
        // Arrange
        var command = new CancelAppointmentCommand { AppointmentId = Guid.NewGuid() };

        _queries.GetByIdAsync(Arg.Any<AppointmentId>(), Arg.Any<CancellationToken>())
            .Returns(Optional.Option.None<Appointment>());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeTrue();
        result.IfError(err => err.ShouldBeOfType<AppointmentNotFoundException>());
        
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Appointment>(), Arg.Any<CancellationToken>());
    }
}