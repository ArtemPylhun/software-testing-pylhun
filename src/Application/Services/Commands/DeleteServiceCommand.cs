using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Services.Exceptions;
using Domain.Entities;
using Domain.ValueObjects;
using MediatR;

namespace Application.Services.Commands;

public record DeleteServiceCommand : IRequest<Result<Service, ServiceException>>
{
    public Guid ServiceId { get; init; }
}

public class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand, Result<Service, ServiceException>>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IServiceQueries _serviceQueries;

    public DeleteServiceCommandHandler(IServiceRepository serviceRepository, IServiceQueries serviceQueries)
    {
        _serviceRepository = serviceRepository;
        _serviceQueries = serviceQueries;
    }

    public async Task<Result<Service, ServiceException>> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
    {
        var serviceId = new ServiceId(request.ServiceId);
        var serviceOption = await _serviceQueries.GetByIdAsync(serviceId, cancellationToken);

        return await serviceOption.Match(
            async service => await DeleteEntity(service, cancellationToken),
            () => Task.FromResult(Result<Service, ServiceException>.Failure(
                new ServiceNotFoundException(serviceId)))
        );
    }

    private async Task<Result<Service, ServiceException>> DeleteEntity(Service entity, CancellationToken cancellationToken)
    {
        try
        {
            await _serviceRepository.DeleteAsync(entity.Id, cancellationToken);
            return Result<Service, ServiceException>.Success(entity);
        }
        catch (Exception exception)
        {
            return new ServiceUnknownException(entity.Id, exception);
        }
    }
}