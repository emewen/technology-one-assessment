using NumberToWords.Domain;

namespace NumberToWords.Domain.Parsing;

/// <summary>
/// Reads a monetary amount typed by a person.
/// </summary>
public interface IMoneyParser
{
    /// <summary>Gets the longest input this parser accepts.</summary>
    int MaxInputLength { get; }

    /// <summary>
    /// Reads <paramref name="input"/> as an amount.
    /// </summary>
    /// <param name="input">The text the person typed.</param>
    /// <returns>
    /// The amount, or the reason it was refused. Bad input never throws.
    /// </returns>
    Result<Money> Parse(string? input);
}
