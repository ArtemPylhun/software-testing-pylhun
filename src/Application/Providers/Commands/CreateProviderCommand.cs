using Application.Common;
using Application.Common.Interfaces.Repositories;
using Application.Providers.Exceptions;
using Domain.Entities;
using Domain.ValueObjects;
using MediatR;

namespace Application.Providers.Commands;

public record CreateProviderCommand : IRequest<Result<Provider, ProviderException>>
{
    public string Name { get; init; }
    public string Specialization { get; init; }
    public string Email { get; init; }
    public TimeOnly StartWorkingHours { get; init; }
    public TimeOnly EndWorkingHours { get; init; }
}

public class CreateProviderCommandHandler : IRequestHandler<CreateProviderCommand, Result<Provider, ProviderException>>
{
    private readonly IProviderRepository _providerRepository;

    public CreateProviderCommandHandler(IProviderRepository providerRepository)
    {
        _providerRepository = providerRepository;
    }

    public async Task<Result<Provider, ProviderException>> Handle(CreateProviderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var provider = Provider.Create(
                request.Name, 
                request.Specialization, 
                request.Email, 
                request.StartWorkingHours, 
                request.EndWorkingHours);

            return await CreateEntity(provider, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return Result<Provider, ProviderException>.Failure(new ProviderDomainException(ex.Message, ex));
        }
    }

    private async Task<Result<Provider, ProviderException>> CreateEntity(Provider entity, CancellationToken cancellationToken)
    {
        try
        {
            await _providerRepository.AddAsync(entity, cancellationToken);
            return Result<Provider, ProviderException>.Success(entity);
        }
        catch (Exception exception)
        {
            return new ProviderUnknownException(entity.Id, exception);
        }
    }
}