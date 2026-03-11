using Application.Appointments.Exceptions;
using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using MediatR;

namespace Application.Appointments.Commands;

public record UpdateAppointmentCommand : IRequest<Result<Appointment, AppointmentException>>
{
    public Guid AppointmentId { get; init; }
    public Guid ProviderId { get; init; }
    public Guid ServiceId { get; init; }
    public DateOnly Date { get; init; }
    public TimeOnly StartTime { get; init; }
}

public class UpdateAppointmentCommandHandler : IRequestHandler<UpdateAppointmentCommand, Result<Appointment, AppointmentException>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAppointmentQueries _appointmentQueries;
    private readonly IProviderQueries _providerQueries;
    private readonly IServiceQueries _serviceQueries;

    public UpdateAppointmentCommandHandler(
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

    public async Task<Result<Appointment, AppointmentException>> Handle(UpdateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointmentId = new AppointmentId(request.AppointmentId);
        var appointmentOption = await _appointmentQueries.GetByIdAsync(appointmentId, cancellationToken);

        return await appointmentOption.Match(
            async appointment =>
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
                                    // 1. Оновлюємо стейт у домені (перевіряє години, статус тощо)
                                    appointment.UpdateDetails(provider, service, request.Date, request.StartTime);

                                    // 2. Перевіряємо перетини (Overlap)
                                    var schedule = await _appointmentQueries.GetScheduleByProviderAsync(provider.Id, request.Date, cancellationToken);
                                    
                                    bool hasOverlap = schedule.Any(a =>
                                        a.Id != appointment.Id && // ІГНОРУЄМО САМОГО СЕБЕ
                                        a.Status != AppointmentStatus.Cancelled &&
                                        a.StartTime < appointment.EndTime &&
                                        a.EndTime > appointment.StartTime);

                                    if (hasOverlap)
                                    {
                                        return await Task.FromResult(
                                            Result<Appointment, AppointmentException>.Failure(
                                                new AppointmentOverlapException(request.Date, appointment.StartTime, appointment.EndTime)));
                                    }

                                    return await UpdateEntity(appointment, cancellationToken);
                                }
                                catch (InvalidOperationException ex)
                                {
                                    return await Task.FromResult(
                                        Result<Appointment, AppointmentException>.Failure(
                                            new AppointmentDomainException(ex.Message, ex)));
                                }
                            },
                            () => Task.FromResult(Result<Appointment, AppointmentException>.Failure(
                                new AppointmentProviderNotFoundException(new ProviderId(request.ProviderId))))
                        );
                    },
                    () => Task.FromResult(Result<Appointment, AppointmentException>.Failure(
                        new AppointmentServiceNotFoundException(new ServiceId(request.ServiceId))))
                );
            },
            () => Task.FromResult(Result<Appointment, AppointmentException>.Failure(
                new AppointmentNotFoundException(appointmentId)))
        );
    }

    private async Task<Result<Appointment, AppointmentException>> UpdateEntity(Appointment entity, CancellationToken cancellationToken)
    {
        try
        {
            await _appointmentRepository.UpdateAsync(entity, cancellationToken);
            return Result<Appointment, AppointmentException>.Success(entity);
        }
        catch (Exception exception)
        {
            return new AppointmentUnknownException(entity.Id, exception);
        }
    }
}