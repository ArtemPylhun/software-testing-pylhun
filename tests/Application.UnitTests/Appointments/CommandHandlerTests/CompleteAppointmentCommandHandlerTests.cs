using System.Runtime.CompilerServices;
using Application.Appointments.Commands;
using Application.Appointments.Exceptions;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using System.Runtime.Serialization;

namespace Application.UnitTests.Appointments.CommandHandlerTests;

public class CompleteAppointmentCommandHandlerTests
{
    private readonly IAppointmentRepository _repository;
    private readonly IAppointmentQueries _queries;
    private readonly CompleteAppointmentCommandHandler _sut;

    public CompleteAppointmentCommandHandlerTests()
    {
        _repository = Substitute.For<IAppointmentRepository>();
        _queries = Substitute.For<IAppointmentQueries>();

        _sut = new CompleteAppointmentCommandHandler(_repository, _queries);
    }

    // Допоміжний метод для обходу інкапсуляції, якщо немає зручного фабричного методу
    private Appointment CreateDummyAppointment(Guid id)
    {
        var appointment = (Appointment)RuntimeHelpers.GetUninitializedObject(typeof(Appointment));
        var idProperty = typeof(Appointment).GetProperty(nameof(Appointment.Id));
        idProperty?.SetValue(appointment, new AppointmentId(id));
        return appointment;
    }

    [Fact]
    public async Task Handle_WhenAppointmentExists_CompletesAndUpdate()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var command = new CompleteAppointmentCommand { AppointmentId = appointmentId };
        var appointment = CreateDummyAppointment(appointmentId);

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
        var command = new CompleteAppointmentCommand { AppointmentId = Guid.NewGuid() };

        _queries.GetByIdAsync(Arg.Any<AppointmentId>(), Arg.Any<CancellationToken>())
            .Returns(Optional.Option.None<Appointment>()); // Або ваш еквівалент None

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeTrue();
        result.IfError(err => err.ShouldBeOfType<AppointmentNotFoundException>());
        
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Appointment>(), Arg.Any<CancellationToken>());
    }
}