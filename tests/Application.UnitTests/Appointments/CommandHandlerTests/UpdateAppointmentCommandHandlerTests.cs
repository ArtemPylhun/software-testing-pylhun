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

public class UpdateAppointmentCommandHandlerTests
{
    private readonly IAppointmentRepository _repository;
    private readonly IAppointmentQueries _appointmentQueries;
    private readonly IProviderQueries _providerQueries;
    private readonly IServiceQueries _serviceQueries;
    private readonly UpdateAppointmentCommandHandler _sut;

    public UpdateAppointmentCommandHandlerTests()
    {
        _repository = Substitute.For<IAppointmentRepository>();
        _appointmentQueries = Substitute.For<IAppointmentQueries>();
        _providerQueries = Substitute.For<IProviderQueries>();
        _serviceQueries = Substitute.For<IServiceQueries>();

        _sut = new UpdateAppointmentCommandHandler(
            _repository,
            _appointmentQueries,
            _providerQueries,
            _serviceQueries);
    }

    // --- Helpers ---
    private Provider CreateDummyProvider(Guid id)
    {
        var provider = (Provider)RuntimeHelpers.GetUninitializedObject(typeof(Provider));
        typeof(Provider).GetProperty(nameof(Provider.Id))?.SetValue(provider, new ProviderId(id));
        return provider;
    }

    private Service CreateDummyService(Guid id)
    {
        var service = (Service)RuntimeHelpers.GetUninitializedObject(typeof(Service));
        typeof(Service).GetProperty(nameof(Service.Id))?.SetValue(service, new ServiceId(id));
        return service;
    }

    private Appointment CreateDummyAppointment(Guid id, TimeOnly startTime, TimeOnly endTime)
    {
        var appointment = (Appointment)RuntimeHelpers.GetUninitializedObject(typeof(Appointment));
        typeof(Appointment).GetProperty(nameof(Appointment.Id))?.SetValue(appointment, new AppointmentId(id));
        typeof(Appointment).GetProperty(nameof(Appointment.StartTime))?.SetValue(appointment, startTime);
        typeof(Appointment).GetProperty(nameof(Appointment.EndTime))?.SetValue(appointment, endTime);
        return appointment;
    }

    // --- Tests ---

    [Fact]
    public async Task Handle_WhenAppointmentNotFound_ReturnsAppointmentNotFoundException()
    {
        var command = new UpdateAppointmentCommand { AppointmentId = Guid.NewGuid() };
        _appointmentQueries.GetByIdAsync(Arg.Any<AppointmentId>(), Arg.Any<CancellationToken>())
            .Returns(Optional.Option.None<Appointment>());

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.IfError(err => err.ShouldBeOfType<AppointmentNotFoundException>());
    }

    [Fact]
    public async Task Handle_WhenServiceNotFound_ReturnsAppointmentServiceNotFoundException()
    {
        var command = new UpdateAppointmentCommand { AppointmentId = Guid.NewGuid(), ServiceId = Guid.NewGuid() };
        
        _appointmentQueries.GetByIdAsync(Arg.Any<AppointmentId>(), Arg.Any<CancellationToken>())
            .Returns(Optional.OptionExtensions.Some(CreateDummyAppointment(command.AppointmentId, new TimeOnly(10, 0), new TimeOnly(11, 0))));
        
        _serviceQueries.GetByIdAsync(Arg.Any<ServiceId>(), Arg.Any<CancellationToken>())
            .Returns(Optional.Option.None<Service>());

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.IfError(err => err.ShouldBeOfType<AppointmentServiceNotFoundException>());
    }

    [Fact]
    public async Task Handle_WhenProviderNotFound_ReturnsAppointmentProviderNotFoundException()
    {
        var command = new UpdateAppointmentCommand { AppointmentId = Guid.NewGuid(), ServiceId = Guid.NewGuid(), ProviderId = Guid.NewGuid() };
        
        _appointmentQueries.GetByIdAsync(Arg.Any<AppointmentId>(), Arg.Any<CancellationToken>())
            .Returns(Optional.OptionExtensions.Some(CreateDummyAppointment(command.AppointmentId, new TimeOnly(10, 0), new TimeOnly(11, 0))));
        
        _serviceQueries.GetByIdAsync(Arg.Any<ServiceId>(), Arg.Any<CancellationToken>())
            .Returns(Optional.OptionExtensions.Some(CreateDummyService(command.ServiceId)));

        _providerQueries.GetByIdAsync(Arg.Any<ProviderId>(), Arg.Any<CancellationToken>())
            .Returns(Optional.Option.None<Provider>());

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.IfError(err => err.ShouldBeOfType<AppointmentProviderNotFoundException>());
    }
}