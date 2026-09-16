using System.Globalization;
using System.Text;

namespace NumberToWords.Domain.Conversion;

/// <summary>
/// Names a whole number in English, following Australian and British usage.
/// </summary>
/// <remarks>
/// <para>
/// Split the digits into groups of three from the right. Name each group with the same rules
/// that apply to any number under a thousand. Append its scale word. The difficult part of
/// the problem, the range 1 to 999, then gets written once and every larger number reuses it.
/// </para>
/// <para>
/// Inside a group, "AND" separates the hundreds figure from the remainder: "ONE HUNDRED AND
/// TWENTY-THREE". Across groups it precedes a final remainder below one hundred: "ONE
/// THOUSAND AND ONE". American usage drops both. The example in the brief keeps the first,
/// so this follows Australian usage.
/// </para>
/// </remarks>
public sealed class EnglishWholeNumberToWordsConverter : IWholeNumberToWordsConverter
{
    /// <inheritdoc />
    public int MaxDigits => EnglishNumberWords.MaxDigits;

    /// <inheritdoc />
    public string Convert(string digits)
    {
        ArgumentNullException.ThrowIfNull(digits);
        GuardDigits(digits);

        string padded = PadToWholeGroups(digits);
        int groupCount = padded.Length / EnglishNumberWords.DigitsPerGroup;

        StringBuilder words = new();
        for (int group = 0; group < groupCount; group++)
        {
            int value = ReadGroup(padded, group);
            if (value == 0)
            {
                continue;
            }

            int scale = groupCount - 1 - group;
            AppendSeparator(words, scale, value);
            words.Append(ConvertGroup(value));

            if (scale > 0)
            {
                words.Append(' ').Append(EnglishNumberWords.Scales[scale]);
            }
        }

        return words.Length == 0 ? EnglishNumberWords.Zero : words.ToString();
    }

    private void GuardDigits(string digits)
    {
        if (digits.Length == 0)
        {
            throw new ArgumentException("Supply at least one digit.", nameof(digits));
        }

        foreach (char character in digits)
        {
            if (!char.IsAsciiDigit(character))
            {
                throw new ArgumentException(
                    $"Expected digits only, and found '{character}'.",
                    nameof(digits));
            }
        }

        if (digits.TrimStart('0').Length > MaxDigits)
        {
            throw new ArgumentException(
                $"This converter names numbers up to {MaxDigits} digits.",
                nameof(digits));
        }
    }

    private static string PadToWholeGroups(string digits)
    {
        int remainder = digits.Length % EnglishNumberWords.DigitsPerGroup;
        return remainder == 0
            ? digits
            : digits.PadLeft(digits.Length + EnglishNumberWords.DigitsPerGroup - remainder, '0');
    }

    private static int ReadGroup(string padded, int group)
    {
        int start = group * EnglishNumberWords.DigitsPerGroup;
        ReadOnlySpan<char> span = padded.AsSpan(start, EnglishNumberWords.DigitsPerGroup);
        return int.Parse(span, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Joins a group to the words already built, choosing between a plain space and " AND ".
    /// </summary>
    /// <remarks>
    /// A non-empty builder means a larger group came first, which is the condition for the
    /// cross-group "AND". It applies to the final group alone, and only when that group falls
    /// below one hundred. "ONE THOUSAND AND ONE" gets the word. "ONE THOUSAND ONE HUNDRED"
    /// does not.
    /// </remarks>
    private static void AppendSeparator(StringBuilder words, int scale, int value)
    {
        if (words.Length == 0)
        {
            return;
        }

        bool isTrailingRemainder = scale == 0 && value < 100;
        words.Append(isTrailingRemainder ? $" {EnglishNumberWords.And} " : " ");
    }

    /// <summary>
    /// Names a number from 1 to 999. Every English naming rule lives in here.
    /// </summary>
    private static string ConvertGroup(int value)
    {
        int hundreds = value / 100;
        int remainder = value % 100;

        if (hundreds == 0)
        {
            return ConvertBelowHundred(remainder);
        }

        string hundredsWords = $"{EnglishNumberWords.BelowTwenty[hundreds]} {EnglishNumberWords.Hundred}";
        return remainder == 0
            ? hundredsWords
            : $"{hundredsWords} {EnglishNumberWords.And} {ConvertBelowHundred(remainder)}";
    }

    /// <summary>
    /// Names a number from 1 to 99.
    /// </summary>
    /// <remarks>
    /// Everything below twenty comes from the table, since English gives those their own names.
    /// Above that the tens word carries the value and a hyphen joins any units digit, as in
    /// "FORTY-FIVE".
    /// </remarks>
    private static string ConvertBelowHundred(int value)
    {
        if (value < 20)
        {
            return EnglishNumberWords.BelowTwenty[value];
        }

        string tens = EnglishNumberWords.Tens[value / 10];
        int units = value % 10;

        return units == 0 ? tens : $"{tens}-{EnglishNumberWords.BelowTwenty[units]}";
    }
}
