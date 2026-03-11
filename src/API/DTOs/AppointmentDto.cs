using Domain.Entities;
using Domain.Enums;

namespace API.DTOs;

public record AppointmentDto(
    Guid Id,
    Guid ProviderId,
    Guid ServiceId,
    string ClientName,
    string ClientEmail,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    AppointmentStatus Status,
    ProviderDto? Provider,
    ServiceDto? Service)
{
    public static AppointmentDto FromDomainModel(Appointment appointment) => new(
        Id: appointment.Id.Value,
        ProviderId: appointment.ProviderId.Value,
        ServiceId: appointment.ServiceId.Value,
        ClientName: appointment.ClientName,
        ClientEmail: appointment.ClientEmail,
        Date: appointment.Date,
        StartTime: appointment.StartTime,
        EndTime: appointment.EndTime,
        Status: appointment.Status,
        Provider: appointment.Provider == null ? null : ProviderDto.FromDomainModel(appointment.Provider),
        Service: appointment.Service == null ? null : ServiceDto.FromDomainModel(appointment.Service));
}

public record AppointmentCreateDto(
    Guid ProviderId,
    Guid ServiceId,
    string ClientName,
    string ClientEmail,
    DateOnly Date,
    TimeOnly StartTime);

public record AppointmentUpdateDto(
    Guid Id,
    Guid ProviderId,
    Guid ServiceId,
    DateOnly Date,
    TimeOnly StartTime);