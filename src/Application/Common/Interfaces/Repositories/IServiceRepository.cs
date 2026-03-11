using Domain.Entities;
using Domain.ValueObjects;

namespace Application.Common.Interfaces.Repositories;

public interface IServiceRepository
{
    Task AddAsync(Service service, CancellationToken ct);
    Task UpdateAsync(Service service, CancellationToken ct);
    Task DeleteAsync(ServiceId serviceId, CancellationToken ct);
}