namespace NumberToWords.Api.Infrastructure;

/// <summary>
/// The log messages the conversion endpoint writes.
/// </summary>
/// <remarks>
/// The source generator turns each of these into a cached delegate that checks the level
/// before it touches any argument. A disabled debug message then does no work at all.
/// </remarks>
internal static partial class ConversionLog
{
    /// <summary>
    /// Records a refused amount. The input stays out of the log: it came from a user, and the
    /// failure code already says what was wrong with it.
    /// </summary>
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "Conversion refused with code {ErrorCode}.")]
    public static partial void ConversionRefused(ILogger logger, string errorCode);

    /// <summary>Records a successful conversion.</summary>
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Debug,
        Message = "Converted an amount of {Amount}.")]
    public static partial void Converted(ILogger logger, string amount);
}
