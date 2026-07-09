using System.Net;
using System.Text.Json;
using Axiom.Domain.Exceptions;
using FluentValidation;

namespace Axiom.Api.Middleware;

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
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain rule violation");
            await WriteProblem(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed");
            await WriteProblem(context, HttpStatusCode.UnprocessableEntity, "Validation failed", ex.Errors.Select(e => e.ErrorMessage));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogInformation(ex, "Resource not found");
            await WriteProblem(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteProblem(context, HttpStatusCode.InternalServerError, "An internal error occurred.");
        }
    }

    private static async Task WriteProblem(HttpContext context, HttpStatusCode status, string detail, IEnumerable<string>? errors = null)
    {
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/problem+json";

        var problem = new Dictionary<string, object?>
        {
            ["type"] = $"https://httpstatuses.io/{(int)status}",
            ["title"] = status.ToString(),
            ["status"] = (int)status,
            ["detail"] = detail
        };

        if (errors is not null)
            problem["errors"] = errors;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
