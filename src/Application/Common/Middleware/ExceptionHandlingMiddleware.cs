using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

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
        else if (exception is DbUpdateException dbUpdateEx)
        {
            var baseException = dbUpdateEx.GetBaseException();
            
            // Переводимо повідомлення в нижній регістр, щоб не залежати від великих/маленьких літер
            var errorMessage = baseException.Message.ToLower(); 

            // Перевіряємо на текст помилки унікальності PostgreSQL або на назву нашого індексу
            if (errorMessage.Contains("23505") || 
                errorMessage.Contains("duplicate key value violates unique constraint") ||
                errorMessage.Contains("ix_appointments_provider_id_date_start_time"))
            {
                message = "This time slot is already booked. Please choose another time.";
                response.StatusCode = (int)HttpStatusCode.Conflict; 
            }
            else
            {
                message = "A database error occurred.";
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        var result = JsonSerializer.Serialize(new { message });
        return response.WriteAsync(result);
    }
}