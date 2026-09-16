using System.Globalization;

namespace NumberToWords.Domain.Conversion;

/// <summary>
/// Names a monetary amount by combining a number converter with a set of currency words.
/// </summary>
/// <remarks>
/// <para>
/// This class decides the wording and nothing else: which parts appear, whether each unit
/// name is singular or plural, and where the word "AND" separates them. Naming the numbers
/// belongs to the injected <see cref="IWholeNumberToWordsConverter"/>. The unit words arrive
/// as data.
/// </para>
/// <para>
/// An empty part drops out, so five cents reads "FIVE CENTS" and not "ZERO DOLLARS AND FIVE
/// CENTS". That matches how people write amounts on cheques. Zero has nowhere to go, so it
/// falls back to "ZERO DOLLARS".
/// </para>
/// </remarks>
public sealed class MoneyToWordsConverter : IMoneyToWordsConverter
{
    private readonly IWholeNumberToWordsConverter _numberConverter;
    private readonly CurrencyNames _currency;

    /// <summary>
    /// Creates a converter for one currency.
    /// </summary>
    /// <param name="numberConverter">Names the numeric part of the amount.</param>
    /// <param name="currency">Supplies the whole and fractional unit words.</param>
    /// <exception cref="ArgumentNullException">Either argument is null.</exception>
    public MoneyToWordsConverter(IWholeNumberToWordsConverter numberConverter, CurrencyNames currency)
    {
        ArgumentNullException.ThrowIfNull(numberConverter);
        ArgumentNullException.ThrowIfNull(currency);

        _numberConverter = numberConverter;
        _currency = currency;
    }

    /// <inheritdoc />
    public string Convert(Money money)
    {
        ArgumentNullException.ThrowIfNull(money);

        List<string> parts = new(capacity: 3);

        if (money.IsNegative)
        {
            parts.Add(EnglishNumberWords.Minus);
        }

        // Zero still has to say something, so it keeps the whole part.
        bool includeWholeUnits = !money.HasNoWholeUnits || money.IsZero;
        if (includeWholeUnits)
        {
            parts.Add(WholeUnitWords(money));
        }

        if (money.FractionalUnits > 0)
        {
            if (includeWholeUnits)
            {
                parts.Add(EnglishNumberWords.And);
            }

            parts.Add(FractionalUnitWords(money));
        }

        return string.Join(' ', parts);
    }

    private string WholeUnitWords(Money money) =>
        $"{_numberConverter.Convert(money.WholeUnits)} {_currency.WholeUnitFor(money.WholeUnits)}";

    private string FractionalUnitWords(Money money)
    {
        string count = money.FractionalUnits.ToString(CultureInfo.InvariantCulture);
        return $"{_numberConverter.Convert(count)} {_currency.FractionalUnitFor(money.FractionalUnits)}";
    }
}
