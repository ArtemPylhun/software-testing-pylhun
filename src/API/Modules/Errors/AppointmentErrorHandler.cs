using Application.Appointments.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace API.Modules.Errors;

public static class AppointmentErrorHandler
{
    public static ObjectResult ToObjectResult(this AppointmentException exception)
    {
        return new ObjectResult(exception.Message)
        {
            StatusCode = exception switch
            {
                AppointmentNotFoundException or 
                    AppointmentProviderNotFoundException or 
                    AppointmentServiceNotFoundException => StatusCodes.Status404NotFound,
                
                AppointmentOverlapException => StatusCodes.Status409Conflict,
                
                AppointmentDomainException or 
                    AppointmentCancellationException or 
                    AppointmentCompletionException => StatusCodes.Status400BadRequest,
                
                AppointmentUnknownException => StatusCodes.Status500InternalServerError,
                
                _ => throw new NotImplementedException("Appointment error handler is not implemented")
            }
        };
    }
}