using Domain.Entities;
using Domain.ValueObjects;
using Optional;

namespace Application.Common.Interfaces.Queries;

public interface IServiceQueries
{
    Task<List<Service>> GetAllAsync(CancellationToken ct = default);
    Task<Option<Service>> GetByIdAsync(ServiceId id, CancellationToken ct = default);
}