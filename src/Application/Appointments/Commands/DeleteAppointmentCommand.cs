using Application.Appointments.Exceptions;
using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.ValueObjects;
using MediatR;

namespace Application.Appointments.Commands;

public record DeleteAppointmentCommand : IRequest<Result<Appointment, AppointmentException>>
{
    public Guid AppointmentId { get; init; }
}

public class DeleteAppointmentCommandHandler : IRequestHandler<DeleteAppointmentCommand, Result<Appointment, AppointmentException>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAppointmentQueries _appointmentQueries;

    public DeleteAppointmentCommandHandler(IAppointmentRepository appointmentRepository, IAppointmentQueries appointmentQueries)
    {
        _appointmentRepository = appointmentRepository;
        _appointmentQueries = appointmentQueries;
    }

    public async Task<Result<Appointment, AppointmentException>> Handle(DeleteAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointmentId = new AppointmentId(request.AppointmentId);
        var appointmentOption = await _appointmentQueries.GetByIdAsync(appointmentId, cancellationToken);

        return await appointmentOption.Match(
            async appointment => await DeleteEntity(appointment, cancellationToken),
            () => Task.FromResult(Result<Appointment, AppointmentException>.Failure(
                new AppointmentNotFoundException(appointmentId)))
        );
    }

    private async Task<Result<Appointment, AppointmentException>> DeleteEntity(Appointment entity, CancellationToken cancellationToken)
    {
        try
        {
            await _appointmentRepository.DeleteAsync(entity.Id, cancellationToken);
            return Result<Appointment, AppointmentException>.Success(entity);
        }
        catch (Exception exception)
        {
            return new AppointmentUnknownException(entity.Id, exception);
        }
    }
}