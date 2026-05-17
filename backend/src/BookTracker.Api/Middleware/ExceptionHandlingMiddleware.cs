using System.Text.Json;
using BookTracker.Application.Common;
using BookTracker.Domain.Common;

namespace BookTracker.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (ValidationException ex)
        {
            await WriteValidationAsync(ctx, ex);
        }
        catch (NotFoundException ex)
        {
            await WriteProblemAsync(ctx, StatusCodes.Status404NotFound, "Not Found", ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteProblemAsync(ctx, StatusCodes.Status409Conflict, "Conflict", ex.Message);
        }
        catch (UnauthorizedException ex)
        {
            await WriteProblemAsync(ctx, StatusCodes.Status401Unauthorized, "Unauthorized", ex.Message);
        }
        catch (DomainException ex)
        {
            await WriteProblemAsync(ctx, StatusCodes.Status400BadRequest, "Bad Request", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteProblemAsync(ctx, StatusCodes.Status500InternalServerError, "Server Error", "An unexpected error occurred.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext ctx, int status, string title, string detail)
    {
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/problem+json";
        var payload = new { type = $"https://httpstatuses.io/{status}", title, status, detail, traceId = ctx.TraceIdentifier };
        await JsonSerializer.SerializeAsync(ctx.Response.Body, payload, JsonOpts);
    }

    private static async Task WriteValidationAsync(HttpContext ctx, ValidationException ex)
    {
        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
        ctx.Response.ContentType = "application/problem+json";
        var payload = new
        {
            type = "https://httpstatuses.io/400",
            title = "Validation Failed",
            status = 400,
            errors = ex.Errors,
            traceId = ctx.TraceIdentifier
        };
        await JsonSerializer.SerializeAsync(ctx.Response.Body, payload, JsonOpts);
    }
}
