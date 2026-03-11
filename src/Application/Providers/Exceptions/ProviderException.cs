using Domain.ValueObjects;

namespace Application.Providers.Exceptions;

public class ProviderException(ProviderId id, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public ProviderId ProviderId { get; } = id;
}

public class ProviderNotFoundException(ProviderId id)
    : ProviderException(id, $"Provider under id: {id} not found!");

public class ProviderUnknownException(ProviderId id, Exception innerException)
    : ProviderException(id, $"Unknown exception for the provider under id: {id} occurred!", innerException);
    
public class ProviderDomainException(string message, Exception innerException)
    : ProviderException(ProviderId.Empty(), message, innerException);