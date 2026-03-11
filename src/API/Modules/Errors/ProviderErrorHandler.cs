using Application.Providers.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace API.Modules.Errors;

public static class ProviderErrorHandler
{
    public static ObjectResult ToObjectResult(this ProviderException exception)
    {
        return new ObjectResult(exception.Message)
        {
            StatusCode = exception switch
            {
                ProviderNotFoundException => StatusCodes.Status404NotFound,
                ProviderDomainException => StatusCodes.Status400BadRequest,
                ProviderUnknownException => StatusCodes.Status500InternalServerError,
                _ => throw new NotImplementedException("Provider error handler is not implemented")
            }
        };
    }
}