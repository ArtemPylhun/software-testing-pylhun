using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Providers.Exceptions;
using Domain.Entities;
using Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Application.Providers.Commands;

public class DeleteProviderCommandValidator : AbstractValidator<DeleteProviderCommand>
{
    public DeleteProviderCommandValidator()
    {
        RuleFor(x => x.ProviderId).NotEmpty();
    }
}

public class DeleteProviderCommandHandler : IRequestHandler<DeleteProviderCommand, Result<Provider, ProviderException>>
{
    private readonly IProviderRepository _providerRepository;
    private readonly IProviderQueries _providerQueries;

    public DeleteProviderCommandHandler(IProviderRepository providerRepository, IProviderQueries providerQueries)
    {
        _providerRepository = providerRepository;
        _providerQueries = providerQueries;
    }

    public async Task<Result<Provider, ProviderException>> Handle(DeleteProviderCommand request, CancellationToken cancellationToken)
    {
        var providerId = new ProviderId(request.ProviderId);
        var providerOption = await _providerQueries.GetByIdAsync(providerId, cancellationToken);

        return await providerOption.Match(
            async provider => await DeleteEntity(provider, cancellationToken),
            () => Task.FromResult(Result<Provider, ProviderException>.Failure(
                new ProviderNotFoundException(providerId)))
        );
    }

    private async Task<Result<Provider, ProviderException>> DeleteEntity(Provider entity, CancellationToken cancellationToken)
    {
        try
        {
            await _providerRepository.DeleteAsync(entity.Id, cancellationToken);
            return Result<Provider, ProviderException>.Success(entity); // Або можна повертати Result<bool>
        }
        catch (Exception exception)
        {
            return new ProviderUnknownException(entity.Id, exception);
        }
    }
}