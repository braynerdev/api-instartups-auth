using Api.Instartups.Auth.DTOs;
using Api.Instartups.Auth.Exceptions;
using Api.Instartups.Auth.Exceptions.Base;
using FastExpressionCompiler;

namespace Api.Instartups.Auth.Middleware;

public class ExceptionsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionsMiddleware> _logger;

    public ExceptionsMiddleware(RequestDelegate next, ILogger<ExceptionsMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = GetStatusCode(exception);

        LogError(exception, statusCode);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var body = ConstructResponseBody(exception);
        await context.Response.WriteAsJsonAsync(body);
    }

    private int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            FluentValidation.ValidationException => StatusCodes.Status400BadRequest,
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            ForbiddenException => StatusCodes.Status403Forbidden,
            ConflictException => StatusCodes.Status409Conflict,
            OperationCanceledException => StatusCodes.Status499ClientClosedRequest,

            _ => StatusCodes.Status500InternalServerError
        };
    }

    private object ConstructResponseBody(Exception exception)
    {
        return exception switch
        {
            FluentValidation.ValidationException validation =>
            BaseResponseDTO<IEnumerable<ValidateErrorDTO>>.Error(
                validation.Errors
                    .GroupBy(x => x.PropertyName)
                    .Select(group => new ValidateErrorDTO(
                        group.Key,
                        group.First().ErrorCode,
                        group
                            .Select(x => x.ErrorMessage)
                            .ToList()
                    )),
            "Erro de validação."
            ),

            IdentityValidationException validation =>
                BaseResponseDTO<IEnumerable<ValidateErrorDTO>>.Error(
                        validation.Error
                    ),
            
            OperationCanceledException =>
                BaseResponseDTO<string>.Error(
                    "Requisição cancelada pelo cliente"),

            _ => BaseResponseDTO<string>.Error(
                    "Erro inesperado")
        };
    }

    private void LogError(Exception exception, int statusCode)
    {
        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Erro interno da aplicação");
        }
        else
        {
            _logger.LogWarning($"Erro de negócio: {exception.Message}");
        }
    }
}