using Application.Appointments.Exceptions;
using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.ValueObjects;
using MediatR;

namespace Application.Appointments.Commands;

public record CancelAppointmentCommand : IRequest<Result<Appointment, AppointmentException>>
{
    public Guid AppointmentId { get; init; }
}

public class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand, Result<Appointment, AppointmentException>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAppointmentQueries _appointmentQueries;

    public CancelAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IAppointmentQueries appointmentQueries)
    {
        _appointmentRepository = appointmentRepository;
        _appointmentQueries = appointmentQueries;
    }

    public async Task<Result<Appointment, AppointmentException>> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointmentId = new AppointmentId(request.AppointmentId);
        var appointmentOption = await _appointmentQueries.GetByIdAsync(appointmentId, cancellationToken);

        return await appointmentOption.Match(
            async appointment =>
            {
                try
                {
                    // Викликаємо метод домену для зміни стану
                    appointment.Cancel(DateTime.UtcNow);
                    
                    return await UpdateEntity(appointment, cancellationToken);
                }
                catch (AppointmentCancellationException ex) // Якщо цей ексепшн викидається прямо з домену
                {
                    return await Task.FromResult(Result<Appointment, AppointmentException>.Failure(ex));
                }
                catch (InvalidOperationException ex) // Альтернативний перехват бізнес-правил
                {
                    return await Task.FromResult(Result<Appointment, AppointmentException>.Failure(
                        new AppointmentCancellationException(appointment.Id, ex.Message)));
                }
            },
            () => Task.FromResult(Result<Appointment, AppointmentException>.Failure(
                new AppointmentNotFoundException(appointmentId)))
        );
    }

    private async Task<Result<Appointment, AppointmentException>> UpdateEntity(
        Appointment entity,
        CancellationToken cancellationToken)
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