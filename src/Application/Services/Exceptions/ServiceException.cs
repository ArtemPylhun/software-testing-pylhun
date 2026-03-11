using Domain.ValueObjects;

namespace Application.Services.Exceptions;

public class ServiceException(ServiceId id, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public ServiceId ServiceId { get; } = id;
}

public class ServiceDomainException(string message, Exception innerException)
    : ServiceException(ServiceId.Empty(), message, innerException);

public class ServiceNotFoundException(ServiceId id)
    : ServiceException(id, $"Service under id: {id} not found!");

public class ServiceUnknownException(ServiceId id, Exception innerException)
    : ServiceException(id, $"Unknown exception for the service under id: {id} occurred!", innerException);