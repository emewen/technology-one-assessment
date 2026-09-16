namespace NumberToWords.Domain.Conversion;

/// <summary>
/// Turns a monetary amount into the words that would be written on a cheque.
/// </summary>
public interface IMoneyToWordsConverter
{
    /// <summary>
    /// Converts <paramref name="money"/> into words, in upper case.
    /// </summary>
    /// <param name="money">The amount to name.</param>
    /// <returns>The amount in words, such as "ONE HUNDRED AND TWENTY-THREE DOLLARS AND FORTY-FIVE CENTS".</returns>
    /// <exception cref="ArgumentNullException"><paramref name="money"/> is null.</exception>
    string Convert(Money money);
}
