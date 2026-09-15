using System.Net;
using System.Text.Json;
using Library.Service.Implementations;
using Microsoft.EntityFrameworkCore;

namespace Library.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? string.Empty;
            _logger.LogWarning(ex, "Concurrency conflict. CorrelationId {CorrelationId}", correlationId);

            await WriteResponseAsync(context, HttpStatusCode.Conflict,
                "הספר נתפס על ידי משתמש אחר. נסה שוב", correlationId);
        }
        catch (LoanConcurrencyException ex)
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? string.Empty;
            _logger.LogWarning(ex, "Concurrency conflict. CorrelationId {CorrelationId}", correlationId);

            await WriteResponseAsync(context, HttpStatusCode.Conflict, ex.Message, correlationId);
        }
        catch (AuthenticationFailedException ex)
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? string.Empty;
            await WriteResponseAsync(context, HttpStatusCode.Unauthorized, ex.Message, correlationId);
        }
        catch (KeyNotFoundException ex)
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? string.Empty;
            await WriteResponseAsync(context, HttpStatusCode.NotFound, ex.Message, correlationId);
        }
        catch (UnauthorizedAccessException ex)
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? string.Empty;
            await WriteResponseAsync(context, HttpStatusCode.Forbidden, ex.Message, correlationId);
        }
        catch (InvalidOperationException ex)
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? string.Empty;
            await WriteResponseAsync(context, HttpStatusCode.BadRequest, ex.Message, correlationId);
        }
        catch (Exception ex)
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? string.Empty;
            _logger.LogError(ex, "Unhandled exception. CorrelationId {CorrelationId}", correlationId);

            await WriteResponseAsync(context, HttpStatusCode.InternalServerError,
                "אירעה שגיאה בלתי צפויה בשרת", correlationId);
        }
    }

    private static async Task WriteResponseAsync(HttpContext context, HttpStatusCode statusCode, string message, string correlationId)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = JsonSerializer.Serialize(new
        {
            statusCode = (int)statusCode,
            message,
            correlationId
        });

        await context.Response.WriteAsync(payload);
    }
}
