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

public class DeleteAppointmentCommandHandlerTests
{
    private readonly IAppointmentRepository _repository;
    private readonly IAppointmentQueries _queries;
    private readonly DeleteAppointmentCommandHandler _sut;

    public DeleteAppointmentCommandHandlerTests()
    {
        _repository = Substitute.For<IAppointmentRepository>();
        _queries = Substitute.For<IAppointmentQueries>();
        _sut = new DeleteAppointmentCommandHandler(_repository, _queries);
    }

    private Appointment CreateDummyAppointment(Guid id)
    {
        var appointment = (Appointment)RuntimeHelpers.GetUninitializedObject(typeof(Appointment));
        typeof(Appointment).GetProperty(nameof(Appointment.Id))?.SetValue(appointment, new AppointmentId(id));
        return appointment;
    }

    [Fact]
    public async Task Handle_WhenAppointmentExists_DeletesAndReturnsSuccess()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var command = new DeleteAppointmentCommand { AppointmentId = appointmentId };
        var appointment = CreateDummyAppointment(appointmentId);

        _queries.GetByIdAsync(Arg.Any<AppointmentId>(), Arg.Any<CancellationToken>())
            .Returns(Optional.OptionExtensions.Some(appointment));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _repository.Received(1).DeleteAsync(appointment.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAppointmentNotFound_ReturnsError()
    {
        // Arrange
        var command = new DeleteAppointmentCommand { AppointmentId = Guid.NewGuid() };

        _queries.GetByIdAsync(Arg.Any<AppointmentId>(), Arg.Any<CancellationToken>())
            .Returns(Optional.Option.None<Appointment>());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeTrue();
        result.IfError(err => err.ShouldBeOfType<AppointmentNotFoundException>());
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<AppointmentId>(), Arg.Any<CancellationToken>());
    }
}