using Microsoft.AspNetCore.Diagnostics;

namespace NumberToWords.Api.Infrastructure;

/// <summary>
/// Turns any unhandled exception into a problem document.
/// </summary>
/// <remarks>
/// <para>
/// A body the model binder cannot read throws <see cref="BadHttpRequestException"/>, which
/// already has the right status on it, so that status gets used. Anything else is a fault
/// in this service and becomes a 500.
/// </para>
/// <para>
/// The client learns what kind of failure it was and nothing more. Exception messages and
/// stack traces go to the log, where they help. None of it reaches a caller who goes
/// looking.
/// </para>
/// </remarks>
internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(problemDetailsService);
        ArgumentNullException.ThrowIfNull(logger);

        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);

        bool isCallerFault = exception is BadHttpRequestException;
        int statusCode = exception is BadHttpRequestException badRequest
            ? badRequest.StatusCode
            : StatusCodes.Status500InternalServerError;

        if (isCallerFault)
        {
            ExceptionLog.RequestRejected(_logger, statusCode);
        }
        else
        {
            ExceptionLog.RequestFailed(_logger, httpContext.Request.Path, exception);
        }

        httpContext.Response.StatusCode = statusCode;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = statusCode,
                Title = isCallerFault ? "The request could not be read." : "The request could not be completed.",
                Detail = isCallerFault
                    ? "Send a JSON body of the form {\"value\": \"123.45\"}."
                    : "Something went wrong at our end. Try again shortly.",
                Extensions = { ["code"] = isCallerFault ? "request.malformed" : "server.error" },
            },
        }).ConfigureAwait(false);
    }
}
