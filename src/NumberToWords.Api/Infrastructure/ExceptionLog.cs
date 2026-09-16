namespace NumberToWords.Api.Infrastructure;

/// <summary>
/// The log messages the exception handler writes.
/// </summary>
internal static partial class ExceptionLog
{
    /// <summary>
    /// Records a request this service could not read. The caller's mistake, so no exception
    /// detail goes with it.
    /// </summary>
    [LoggerMessage(
        EventId = 2000,
        Level = LogLevel.Information,
        Message = "Rejected a malformed request with status {StatusCode}.")]
    public static partial void RequestRejected(ILogger logger, int statusCode);

    /// <summary>Records a fault in this service, with everything needed to diagnose it.</summary>
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Error,
        Message = "Unhandled exception while serving {Path}.")]
    public static partial void RequestFailed(ILogger logger, string path, Exception exception);
}
