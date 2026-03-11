using Domain.Entities;
using Domain.ValueObjects;
using Optional;

namespace Application.Common.Interfaces.Queries;

public interface IProviderQueries
{
    Task<List<Provider>> GetAllAsync(CancellationToken ct = default);
    Task<Option<Provider>> GetByIdAsync(ProviderId id, CancellationToken ct = default);
    Task<List<TimeOnly>> GetAvailableSlotsAsync(ProviderId id, DateOnly date, CancellationToken ct = default);
}