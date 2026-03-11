using Application.Common;
using Application.Common.Interfaces.Repositories;
using Application.Services.Exceptions;
using Domain.Entities;
using Domain.ValueObjects;
using MediatR;

namespace Application.Services.Commands;

public record CreateServiceCommand : IRequest<Result<Service, ServiceException>>
{
    public string Name { get; init; }
    public int DurationMinutes { get; init; }
    public decimal Price { get; init; }
    public string Description { get; init; }
}

public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, Result<Service, ServiceException>>
{
    private readonly IServiceRepository _serviceRepository;

    public CreateServiceCommandHandler(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<Result<Service, ServiceException>> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var service = Service.Create(
                request.Name, 
                request.DurationMinutes, 
                request.Price, 
                request.Description);

            return await CreateEntity(service, cancellationToken);
        }
        catch (ArgumentException ex) // Відловлюємо валідацію домену (Duration <= 0, Price <= 0)
        {
            return Result<Service, ServiceException>.Failure(new ServiceDomainException(ex.Message, ex));
        }
    }

    private async Task<Result<Service, ServiceException>> CreateEntity(Service entity, CancellationToken cancellationToken)
    {
        try
        {
            await _serviceRepository.AddAsync(entity, cancellationToken);
            return Result<Service, ServiceException>.Success(entity);
        }
        catch (Exception exception)
        {
            return new ServiceUnknownException(entity.Id, exception);
        }
    }
}