using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Services.Exceptions;
using Domain.Entities;
using Domain.ValueObjects;
using MediatR;

namespace Application.Services.Commands;

public record UpdateServiceCommand : IRequest<Result<Service, ServiceException>>
{
    public Guid ServiceId { get; init; }
    public string Name { get; init; }
    public int DurationMinutes { get; init; }
    public decimal Price { get; init; }
    public string Description { get; init; }
}

public class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, Result<Service, ServiceException>>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IServiceQueries _serviceQueries;

    public UpdateServiceCommandHandler(IServiceRepository serviceRepository, IServiceQueries serviceQueries)
    {
        _serviceRepository = serviceRepository;
        _serviceQueries = serviceQueries;
    }

    public async Task<Result<Service, ServiceException>> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var serviceId = new ServiceId(request.ServiceId);
        var serviceOption = await _serviceQueries.GetByIdAsync(serviceId, cancellationToken);

        return await serviceOption.Match(
            async service =>
            {
                try
                {
                    service.UpdateDetails(
                        request.Name, 
                        request.DurationMinutes, 
                        request.Price, 
                        request.Description);

                    return await UpdateEntity(service, cancellationToken);
                }
                catch (ArgumentException ex)
                {
                    return await Task.FromResult(Result<Service, ServiceException>.Failure(
                        new ServiceDomainException(ex.Message, ex)));
                }
            },
            () => Task.FromResult(Result<Service, ServiceException>.Failure(
                new ServiceNotFoundException(serviceId)))
        );
    }

    private async Task<Result<Service, ServiceException>> UpdateEntity(Service entity, CancellationToken cancellationToken)
    {
        try
        {
            await _serviceRepository.UpdateAsync(entity, cancellationToken);
            return Result<Service, ServiceException>.Success(entity);
        }
        catch (Exception exception)
        {
            return new ServiceUnknownException(entity.Id, exception);
        }
    }
}