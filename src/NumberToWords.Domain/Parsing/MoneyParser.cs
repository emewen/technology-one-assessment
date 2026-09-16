using System.Globalization;
using System.Text;

namespace NumberToWords.Domain.Parsing;

/// <summary>
/// Reads an amount from text using a grammar written for this purpose.
/// </summary>
/// <remarks>
/// <para>
/// <c>decimal.TryParse</c> was the obvious choice and does not fit. It returns one boolean
/// for every kind of failure, so it can never tell someone which part of their input was
/// wrong. It accepts forms that mean nothing as money, exponent notation such as "1e5"
/// among them. Past 29 significant digits it rounds without saying so. Reading the
/// characters here takes about sixty lines and names a reason for every rejection.
/// </para>
/// <para>
/// Whitespace and thousands separators get stripped before parsing, never validated.
/// Someone pasting "1,234.56" out of a spreadsheet means what someone typing "1234.56"
/// means. Checking that every separator sits three digits apart would reject that for
/// nothing.
/// </para>
/// </remarks>
public sealed class MoneyParser : IMoneyParser
{
    private const char DecimalPoint = '.';
    private const char CurrencySymbol = '$';
    private const int FractionalDigits = 2;

    /// <inheritdoc />
    public int MaxInputLength => 64;

    /// <inheritdoc />
    public Result<Money> Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Result<Money>.Failure(ValidationError.Empty);
        }

        if (input.Length > MaxInputLength)
        {
            return Result<Money>.Failure(ValidationError.TooLong(MaxInputLength));
        }

        string cleaned = RemoveDecoration(input);
        int index = ReadSign(cleaned, out bool isNegative);

        StringBuilder wholeDigits = new();
        StringBuilder fractionalDigits = new();
        bool afterDecimalPoint = false;

        for (; index < cleaned.Length; index++)
        {
            char character = cleaned[index];

            if (char.IsAsciiDigit(character))
            {
                (afterDecimalPoint ? fractionalDigits : wholeDigits).Append(character);
            }
            else if (character == DecimalPoint)
            {
                if (afterDecimalPoint)
                {
                    return Result<Money>.Failure(ValidationError.MultipleDecimalPoints);
                }

                afterDecimalPoint = true;
            }
            else
            {
                return Result<Money>.Failure(ValidationError.InvalidCharacter(character));
            }
        }

        return Build(isNegative, wholeDigits.ToString(), fractionalDigits.ToString());
    }

    /// <summary>
    /// Strips the characters people add for readability, which carry no value.
    /// </summary>
    private static string RemoveDecoration(string input)
    {
        StringBuilder cleaned = new(input.Length);
        foreach (char character in input)
        {
            if (!char.IsWhiteSpace(character) && character != ',')
            {
                cleaned.Append(character);
            }
        }

        return cleaned.ToString();
    }

    /// <summary>
    /// Consumes an optional sign and an optional currency symbol, in either order.
    /// </summary>
    /// <remarks>
    /// Spreadsheets produce both "-$5.00" and "$-5.00". Read both.
    /// </remarks>
    /// <returns>The index of the first character after the prefix.</returns>
    private static int ReadSign(string cleaned, out bool isNegative)
    {
        isNegative = false;
        bool signRead = false;
        bool symbolRead = false;
        int index = 0;

        while (index < cleaned.Length)
        {
            char character = cleaned[index];

            if (!signRead && (character == '-' || character == '+'))
            {
                isNegative = character == '-';
                signRead = true;
            }
            else if (!symbolRead && character == CurrencySymbol)
            {
                symbolRead = true;
            }
            else
            {
                break;
            }

            index++;
        }

        return index;
    }

    private static Result<Money> Build(bool isNegative, string wholeDigits, string fractionalDigits)
    {
        if (wholeDigits.Length == 0 && fractionalDigits.Length == 0)
        {
            return Result<Money>.Failure(ValidationError.NoDigits);
        }

        if (fractionalDigits.Length > FractionalDigits)
        {
            return Result<Money>.Failure(ValidationError.TooManyDecimalPlaces);
        }

        string significantWholeDigits = wholeDigits.TrimStart('0');
        if (significantWholeDigits.Length > Money.MaxWholeUnitDigits)
        {
            return Result<Money>.Failure(ValidationError.TooLarge(Money.MaxWholeUnitDigits));
        }

        // "0.5" means fifty cents, not five, so the fraction pads on the right.
        int fractionalUnits = fractionalDigits.Length == 0
            ? 0
            : int.Parse(fractionalDigits.PadRight(FractionalDigits, '0'), CultureInfo.InvariantCulture);

        return Result<Money>.Success(Money.Create(isNegative, wholeDigits, fractionalUnits));
    }
}
