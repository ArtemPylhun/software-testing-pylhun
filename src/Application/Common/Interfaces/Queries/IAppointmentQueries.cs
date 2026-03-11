using Domain.Entities;
using Domain.ValueObjects;
using Optional;

namespace Application.Common.Interfaces.Queries;

public interface IAppointmentQueries
{
    Task<List<Appointment>> GetScheduleByProviderAsync(ProviderId providerId, DateOnly date, CancellationToken ct = default);
    Task<Option<Appointment>> GetByIdAsync(AppointmentId id, CancellationToken ct = default);
}