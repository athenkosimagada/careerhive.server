using System.Security.Authentication;
using careerhive.domain.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace careerhive.api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next, 
        ILogger<ExceptionHandlingMiddleware> logger, 
        IWebHostEnvironment environment)
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
            _logger.LogError(ex, "An unhandled exception has occurred.");

            context.Response.StatusCode = GetHttpStatusCode(ex);

            var errorResponse = new
            {
                Success = false,
                StatusCode = context.Response.StatusCode,
                Error = GetErrorMessage(ex)
            };

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }

    private int GetHttpStatusCode(Exception ex)
    {
        return ex switch
        {
            SecurityTokenExpiredException _ => StatusCodes.Status401Unauthorized,
            SecurityTokenInvalidSignatureException _ => StatusCodes.Status401Unauthorized,
            SecurityTokenException _ => StatusCodes.Status401Unauthorized,
            AuthenticationException _ => StatusCodes.Status401Unauthorized,
            RefreshTokenStorageException _ => StatusCodes.Status500InternalServerError,
            AlreadyExistsException _ => StatusCodes.Status409Conflict,
            ArgumentException _ => StatusCodes.Status400BadRequest,
            NotFoundException _ => StatusCodes.Status404NotFound,
            UnauthorizedAccessException _ => StatusCodes.Status401Unauthorized,
            LockedOutException _ => StatusCodes.Status403Forbidden,
            TwoFactorRequiredException _ => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };
    }


    private string GetErrorMessage(Exception ex)
    {
        if (ex is AlreadyExistsException || 
            ex is ArgumentException || 
            ex is NotFoundException ||
            ex is UnauthorizedAccessException || 
            ex is LockedOutException || 
            ex is TwoFactorRequiredException || 
            ex is SecurityTokenExpiredException || 
            ex is SecurityTokenInvalidSignatureException || 
            ex is SecurityTokenException || 
            ex is AuthenticationException || 
            ex is RefreshTokenStorageException || 
            ex is IdentityException)
        {
            return ex.Message;
        }

        return "An internal server error has occurred. Please try again later.";
    }
}
