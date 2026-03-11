using Domain.Entities;
using Domain.ValueObjects;

namespace Application.Common.Interfaces.Repositories;

public interface IAppointmentRepository
{
    Task AddAsync(Appointment appointment, CancellationToken ct = default);
    Task UpdateAsync(Appointment appointment, CancellationToken ct = default);
    Task DeleteAsync(AppointmentId appointmentId, CancellationToken ct);
}