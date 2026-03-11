using Domain.Entities;
using Domain.ValueObjects;

namespace Application.Common.Interfaces.Repositories;

public interface IProviderRepository
{
    Task AddAsync(Provider provider, CancellationToken ct = default);
    Task UpdateAsync(Provider provider, CancellationToken ct = default);
    Task DeleteAsync(ProviderId providerId, CancellationToken ct = default);
}