namespace NumberToWords.Domain;

/// <summary>
/// Converts typed text into an amount in words, in one call.
/// </summary>
/// <remarks>
/// Callers outside the domain depend on this instead of on the parser and the converter
/// separately. The order of those steps stays an internal matter.
/// </remarks>
public interface IMoneyConversionService
{
    /// <summary>Gets the longest input this service accepts.</summary>
    int MaxInputLength { get; }

    /// <summary>
    /// Reads <paramref name="input"/> and converts it to words.
    /// </summary>
    /// <param name="input">The text the person typed.</param>
    /// <returns>The conversion, or the reason the input was refused.</returns>
    Result<MoneyConversion> Convert(string? input);
}
