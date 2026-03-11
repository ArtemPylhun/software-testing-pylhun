using Domain.Entities;

namespace API.DTOs;

public record ProviderDto(
    Guid Id,
    string Name,
    string Specialization,
    string Email,
    TimeOnly StartWorkingHours,
    TimeOnly EndWorkingHours)
{
    public static ProviderDto FromDomainModel(Provider provider) => new(
        Id: provider.Id.Value,
        Name: provider.Name,
        Specialization: provider.Specialization,
        Email: provider.Email,
        StartWorkingHours: provider.StartWorkingHours,
        EndWorkingHours: provider.EndWorkingHours);
}

public record ProviderCreateDto(
    string Name,
    string Specialization,
    string Email,
    TimeOnly StartWorkingHours,
    TimeOnly EndWorkingHours);

public record ProviderUpdateDto(
    Guid Id,
    string Name,
    string Specialization,
    string Email,
    TimeOnly StartWorkingHours,
    TimeOnly EndWorkingHours);