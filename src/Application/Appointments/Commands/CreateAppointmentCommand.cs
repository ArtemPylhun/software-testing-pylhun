using Application.Appointments.Exceptions;
using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using MediatR;

namespace Application.Appointments.Commands;

public record CreateAppointmentCommand : IRequest<Result<Appointment, AppointmentException>>
{
    public Guid ProviderId { get; init; }
    public Guid ServiceId { get; init; }
    public string ClientName { get; init; }
    public string ClientEmail { get; init; }
    public DateOnly Date { get; init; }
    public TimeOnly StartTime { get; init; }
}

public class
    CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand,
    Result<Appointment, AppointmentException>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAppointmentQueries _appointmentQueries;
    private readonly IProviderQueries _providerQueries;
    private readonly IServiceQueries _serviceQueries;

    public CreateAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IAppointmentQueries appointmentQueries,
        IProviderQueries providerQueries,
        IServiceQueries serviceQueries)
    {
        _appointmentRepository = appointmentRepository;
        _appointmentQueries = appointmentQueries;
        _providerQueries = providerQueries;
        _serviceQueries = serviceQueries;
    }

    public async Task<Result<Appointment, AppointmentException>> Handle(CreateAppointmentCommand request,
        CancellationToken cancellationToken)
    {
        var serviceOption = await _serviceQueries.GetByIdAsync(new ServiceId(request.ServiceId), cancellationToken);

        return await serviceOption.Match(
            async service =>
            {
                var providerOption = await _providerQueries.GetByIdAsync(new ProviderId(request.ProviderId), cancellationToken);

                return await providerOption.Match(
                    async provider =>
                    {
                        try
                        {
                            var newAppointment = Appointment.Create(
                                provider,
                                service,
                                request.ClientName,
                                request.ClientEmail,
                                request.Date,
                                request.StartTime);

                            var schedule =
                                await _appointmentQueries.GetScheduleByProviderAsync(provider.Id, request.Date,
                                    cancellationToken);

                            if (schedule.Any(a => a.Status != AppointmentStatus.Cancelled &&
                                                  a.StartTime < newAppointment.EndTime &&
                                                  a.EndTime > newAppointment.StartTime))
                            {
                                return await Task.FromResult(
                                    Result<Appointment, AppointmentException>.Failure(
                                        new AppointmentOverlapException(request.Date, newAppointment.StartTime,
                                            newAppointment.EndTime)));
                            }

                            return await CreateEntity(newAppointment, cancellationToken);
                        }
                        catch (InvalidOperationException ex)
                        {
                            return await Task.FromResult(
                                Result<Appointment, AppointmentException>.Failure(
                                    new AppointmentDomainException(ex.Message, ex)));
                        }
                    },
                    () => Task.FromResult(
                        Result<Appointment, AppointmentException>.Failure(
                            new AppointmentProviderNotFoundException(new ProviderId(request.ProviderId))))
                );
            },
            () => Task.FromResult(
                Result<Appointment, AppointmentException>.Failure(
                    new AppointmentServiceNotFoundException(new ServiceId(request.ServiceId))))
        );
    }

    private async Task<Result<Appointment, AppointmentException>> CreateEntity(
        Appointment entity,
        CancellationToken cancellationToken)
    {
        try
        {
            await _appointmentRepository.AddAsync(entity, cancellationToken);
            return Result<Appointment, AppointmentException>.Success(entity);
        }
        catch (Exception exception)
        {
            return new AppointmentUnknownException(AppointmentId.Empty(), exception);
        }
    }
}