using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Application.Common.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        string message = "An unexpected error occurred.";
        response.StatusCode = (int)HttpStatusCode.InternalServerError;

        if (exception is ValidationException validationException)
        {
            message = validationException.Errors.Select(e => e.ErrorMessage).First();
            response.StatusCode = (int)HttpStatusCode.BadRequest;
        }

        var result = JsonSerializer.Serialize(new { message });
        return response.WriteAsync(result);
    }
}