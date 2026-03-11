using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Providers.Exceptions;
using Domain.Entities;
using Domain.ValueObjects;
using MediatR;

namespace Application.Providers.Commands;

public record UpdateProviderCommand : IRequest<Result<Provider, ProviderException>>
{
    public Guid ProviderId { get; init; }
    public string Name { get; init; }
    public string Specialization { get; init; }
    public string Email { get; init; }
    public TimeOnly StartWorkingHours { get; init; }
    public TimeOnly EndWorkingHours { get; init; }
}

public class UpdateProviderCommandHandler : IRequestHandler<UpdateProviderCommand, Result<Provider, ProviderException>>
{
    private readonly IProviderRepository _providerRepository;
    private readonly IProviderQueries _providerQueries;

    public UpdateProviderCommandHandler(IProviderRepository providerRepository, IProviderQueries providerQueries)
    {
        _providerRepository = providerRepository;
        _providerQueries = providerQueries;
    }

    public async Task<Result<Provider, ProviderException>> Handle(UpdateProviderCommand request, CancellationToken cancellationToken)
    {
        var providerId = new ProviderId(request.ProviderId);
        var providerOption = await _providerQueries.GetByIdAsync(providerId, cancellationToken);

        return await providerOption.Match(
            async provider =>
            {
                try
                {
                    provider.UpdateDetails(
                        request.Name, 
                        request.Specialization, 
                        request.Email, 
                        request.StartWorkingHours, 
                        request.EndWorkingHours);

                    return await UpdateEntity(provider, cancellationToken);
                }
                catch (ArgumentException ex)
                {
                    return await Task.FromResult(Result<Provider, ProviderException>.Failure(
                        new ProviderDomainException(ex.Message, ex)));
                }
            },
            () => Task.FromResult(Result<Provider, ProviderException>.Failure(
                new ProviderNotFoundException(providerId)))
        );
    }

    private async Task<Result<Provider, ProviderException>> UpdateEntity(Provider entity, CancellationToken cancellationToken)
    {
        try
        {
            await _providerRepository.UpdateAsync(entity, cancellationToken);
            return Result<Provider, ProviderException>.Success(entity);
        }
        catch (Exception exception)
        {
            return new ProviderUnknownException(entity.Id, exception);
        }
    }
}