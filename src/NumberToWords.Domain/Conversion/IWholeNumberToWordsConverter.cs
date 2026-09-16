namespace NumberToWords.Domain.Conversion;

/// <summary>
/// Turns a non negative whole number into words.
/// </summary>
/// <remarks>
/// Kept apart from the currency wording so the two can change independently. Naming a number
/// belongs to the language. Calling the units dollars belongs to the currency.
/// </remarks>
public interface IWholeNumberToWordsConverter
{
    /// <summary>
    /// Gets the largest number of digits this converter can name.
    /// </summary>
    int MaxDigits { get; }

    /// <summary>
    /// Converts a whole number, supplied as digits, into words.
    /// </summary>
    /// <param name="digits">
    /// The number as ASCII digits. Leading zeros are allowed and carry no meaning.
    /// </param>
    /// <returns>The number in words, in upper case.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="digits"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="digits"/> is empty, holds a character that is not a digit, or holds
    /// more digits than <see cref="MaxDigits"/>.
    /// </exception>
    string Convert(string digits);
}
