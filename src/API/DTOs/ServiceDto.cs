using Domain.Entities;

namespace API.DTOs;

public record ServiceDto(
    Guid Id,
    string Name,
    int DurationMinutes,
    decimal Price,
    string Description)
{
    public static ServiceDto FromDomainModel(Service service) => new(
        Id: service.Id.Value,
        Name: service.Name,
        DurationMinutes: service.DurationMinutes,
        Price: service.Price,
        Description: service.Description);
}

public record ServiceCreateDto(
    string Name,
    int DurationMinutes,
    decimal Price,
    string Description);

public record ServiceUpdateDto(
    Guid Id,
    string Name,
    int DurationMinutes,
    decimal Price,
    string Description);