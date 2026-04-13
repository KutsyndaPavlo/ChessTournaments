using System.Diagnostics;
using System.Net;
using System.Text.Json;
using ChessTournaments.Shared.Infrastructure.Http;
using Microsoft.EntityFrameworkCore;

namespace ChessTournaments.API.Middleware;

/// <summary>
/// Middleware for handling unhandled exceptions globally.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment
    )
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = Activity.Current?.Id ?? context.TraceIdentifier;

        _logger.LogError(
            exception,
            "An unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {Path}",
            correlationId,
            context.Request.Path
        );

        var (statusCode, error) = MapExceptionToResponse(exception, correlationId);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = JsonSerializer.Serialize(
            error,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );

        await context.Response.WriteAsync(response);
    }

    private (HttpStatusCode StatusCode, ErrorResponse Error) MapExceptionToResponse(
        Exception exception,
        string correlationId
    )
    {
        return exception switch
        {
            DbUpdateConcurrencyException => (
                HttpStatusCode.Conflict,
                new ErrorResponse(
                    $"The resource was modified by another user. Please refresh and try again. CorrelationId: {correlationId}"
                )
            ),
            DbUpdateException => (
                HttpStatusCode.BadRequest,
                new ErrorResponse($"A database error occurred. CorrelationId: {correlationId}")
            ),
            UnauthorizedAccessException => (
                HttpStatusCode.Forbidden,
                new ErrorResponse(
                    $"You do not have permission to perform this action. CorrelationId: {correlationId}"
                )
            ),
            ArgumentException or ArgumentNullException => (
                HttpStatusCode.BadRequest,
                new ErrorResponse(
                    _environment.IsDevelopment()
                        ? exception.Message
                        : $"Invalid request. CorrelationId: {correlationId}"
                )
            ),
            InvalidOperationException => (
                HttpStatusCode.BadRequest,
                new ErrorResponse(
                    _environment.IsDevelopment()
                        ? exception.Message
                        : $"Invalid operation. CorrelationId: {correlationId}"
                )
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse(
                    _environment.IsDevelopment()
                        ? $"{exception.Message} CorrelationId: {correlationId}"
                        : $"An unexpected error occurred. CorrelationId: {correlationId}"
                )
            ),
        };
    }
}
