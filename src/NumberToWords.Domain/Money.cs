using System.Globalization;

namespace NumberToWords.Domain;

/// <summary>
/// An exact monetary amount, split into whole units and fractional units.
/// </summary>
/// <remarks>
/// <para>
/// The whole part is a string of digits, not a numeric type. Binary floating point cannot
/// represent most decimal fractions, which rules out <see cref="double"/> for money
/// outright. <see cref="decimal"/> is exact but stops at 29 significant digits. Digits in a
/// string have no ceiling at all, so the only limit left on this type is how many scale
/// names the language supplies.
/// </para>
/// <para>
/// The type carries no currency. <see cref="Conversion.CurrencyNames"/> supplies the unit
/// names at conversion time, so one amount can render as dollars or as pounds.
/// </para>
/// </remarks>
public sealed record Money
{
    /// <summary>The number of fractional units in one whole unit.</summary>
    public const int FractionalUnitsPerWholeUnit = 100;

    /// <summary>
    /// The largest number of digits allowed before the decimal point.
    /// </summary>
    /// <remarks>
    /// Thirty digits is as far as the English scale names reach, ending at octillions.
    /// <c>EnglishWholeNumberToWordsConverter</c> covers exactly that range. A test keeps the
    /// two numbers in step.
    /// </remarks>
    public const int MaxWholeUnitDigits = 30;

    private Money(bool isNegative, string wholeUnits, int fractionalUnits)
    {
        IsNegative = isNegative;
        WholeUnits = wholeUnits;
        FractionalUnits = fractionalUnits;
    }

    /// <summary>Gets a value indicating whether the amount falls below zero.</summary>
    public bool IsNegative { get; }

    /// <summary>
    /// Gets the whole units as digits, with leading zeros removed. An amount under one unit
    /// gives "0".
    /// </summary>
    public string WholeUnits { get; }

    /// <summary>Gets the fractional units, from 0 to 99.</summary>
    public int FractionalUnits { get; }

    /// <summary>Gets a value indicating whether the amount is exactly zero.</summary>
    public bool IsZero => FractionalUnits == 0 && WholeUnits == "0";

    /// <summary>Gets a value indicating whether the amount is under one whole unit.</summary>
    public bool HasNoWholeUnits => WholeUnits == "0";

    /// <summary>
    /// Creates an amount from its parts.
    /// </summary>
    /// <param name="isNegative">Whether the amount falls below zero.</param>
    /// <param name="wholeUnits">
    /// The whole units as digits. Leading zeros and an empty string are both accepted and
    /// normalise to "0".
    /// </param>
    /// <param name="fractionalUnits">The fractional units, from 0 to 99.</param>
    /// <exception cref="ArgumentNullException"><paramref name="wholeUnits"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="wholeUnits"/> holds a character that is not a digit, or too many digits.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="fractionalUnits"/> falls outside 0 to 99.</exception>
    public static Money Create(bool isNegative, string wholeUnits, int fractionalUnits)
    {
        ArgumentNullException.ThrowIfNull(wholeUnits);
        ArgumentOutOfRangeException.ThrowIfNegative(fractionalUnits);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(fractionalUnits, FractionalUnitsPerWholeUnit - 1);

        foreach (char character in wholeUnits)
        {
            if (!char.IsAsciiDigit(character))
            {
                throw new ArgumentException(
                    $"Whole units must hold digits only, and this held '{character}'.",
                    nameof(wholeUnits));
            }
        }

        string normalised = RemoveLeadingZeros(wholeUnits);
        if (normalised.Length > MaxWholeUnitDigits)
        {
            throw new ArgumentException(
                $"Whole units cannot exceed {MaxWholeUnitDigits} digits, and this held {normalised.Length}.",
                nameof(wholeUnits));
        }

        // Drop the sign on an amount of nothing. Nobody writes "MINUS ZERO DOLLARS".
        bool signIsMeaningful = isNegative && !(normalised == "0" && fractionalUnits == 0);

        return new Money(signIsMeaningful, normalised, fractionalUnits);
    }

    /// <summary>Returns the amount in digits, for logs and diagnostics.</summary>
    public override string ToString() => string.Create(
        CultureInfo.InvariantCulture,
        $"{(IsNegative ? "-" : string.Empty)}{WholeUnits}.{FractionalUnits:D2}");

    private static string RemoveLeadingZeros(string digits)
    {
        int firstSignificant = 0;
        while (firstSignificant < digits.Length - 1 && digits[firstSignificant] == '0')
        {
            firstSignificant++;
        }

        return digits.Length == 0 ? "0" : digits[firstSignificant..];
    }
}
