using Application.Common;
using Application.Providers.Exceptions;
using Domain.Entities;
using MediatR;

namespace Application.Providers.Commands;

public record DeleteProviderCommand : IRequest<Result<Provider, ProviderException>>
{
    public Guid ProviderId { get; init; }
}