using Application.Services.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace API.Modules.Errors;

public static class ServiceErrorHandler
{
    public static ObjectResult ToObjectResult(this ServiceException exception)
    {
        return new ObjectResult(exception.Message)
        {
            StatusCode = exception switch
            {
                ServiceNotFoundException => StatusCodes.Status404NotFound,
                ServiceDomainException => StatusCodes.Status400BadRequest,
                ServiceUnknownException => StatusCodes.Status500InternalServerError,
                _ => throw new NotImplementedException("Service error handler is not implemented")
            }
        };
    }
}