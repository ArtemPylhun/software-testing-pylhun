using Domain.ValueObjects;

namespace Application.Appointments.Exceptions;

public class AppointmentException(AppointmentId id, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public AppointmentId AppointmentId { get; } = id;
}

public class AppointmentNotFoundException(AppointmentId id)
    : AppointmentException(id, $"Appointment under id: {id} not found!");

public class AppointmentProviderNotFoundException(ProviderId providerId)
    : AppointmentException(AppointmentId.Empty(), $"Appointment provider under id: {providerId} not found!");

public class AppointmentServiceNotFoundException(ServiceId serviceId)
    : AppointmentException(AppointmentId.Empty(), $"Appointment service under id: {serviceId} not found!");

public class AppointmentOverlapException(DateOnly date, TimeOnly startTime, TimeOnly endTime)
    : AppointmentException(AppointmentId.Empty(), $"Appointment overlap detected. Date: {date}. Time: {startTime} - {endTime}!");

public class AppointmentDomainException(string message, Exception innerException)
    : AppointmentException(AppointmentId.Empty(), message, innerException);

public class AppointmentUnknownException(AppointmentId id, Exception innerException)
    : AppointmentException(id, $"Unknown exception for the appointment under id: {id} occurred!", innerException);
    
public class AppointmentCancellationException(AppointmentId id, string message)
    : AppointmentException(id, $"Failed to cancel appointment {id}: {message}");

public class AppointmentCompletionException(AppointmentId id, string message)
    : AppointmentException(id, $"Failed to complete appointment {id}: {message}");