using System.Collections.ObjectModel;

namespace NumberToWords.Domain.Conversion;

/// <summary>
/// Every English word the converter needs, gathered in one place.
/// </summary>
internal static class EnglishNumberWords
{
    /// <summary>The word for nothing at all.</summary>
    public const string Zero = "ZERO";

    /// <summary>The word joining a hundreds figure to the remainder below it.</summary>
    public const string And = "AND";

    /// <summary>The word for a group of one hundred.</summary>
    public const string Hundred = "HUNDRED";

    /// <summary>The word marking an amount below zero.</summary>
    public const string Minus = "MINUS";

    /// <summary>
    /// Numbers zero through nineteen, indexed by value.
    /// </summary>
    /// <remarks>
    /// The table stops at nineteen because English names each of these separately. No rule
    /// builds THIRTEEN out of THREE.
    /// </remarks>
    public static ReadOnlyCollection<string> BelowTwenty { get; } = new([
        "ZERO", "ONE", "TWO", "THREE", "FOUR", "FIVE", "SIX", "SEVEN", "EIGHT", "NINE",
        "TEN", "ELEVEN", "TWELVE", "THIRTEEN", "FOURTEEN", "FIFTEEN", "SIXTEEN",
        "SEVENTEEN", "EIGHTEEN", "NINETEEN",
    ]);

    /// <summary>
    /// The multiples of ten from twenty to ninety, indexed by the tens digit.
    /// </summary>
    /// <remarks>
    /// The first two slots stay empty so the tens digit indexes straight into the table.
    /// Anything below twenty comes from <see cref="BelowTwenty"/>.
    /// </remarks>
    public static ReadOnlyCollection<string> Tens { get; } = new([
        string.Empty, string.Empty, "TWENTY", "THIRTY", "FORTY", "FIFTY",
        "SIXTY", "SEVENTY", "EIGHTY", "NINETY",
    ]);

    /// <summary>
    /// The name of each three digit group, indexed by how many groups sit to its right.
    /// </summary>
    /// <remarks>
    /// Short scale names, as used in Australia, the United Kingdom and the United States,
    /// where a billion means a thousand million. The long scale still current in parts of
    /// Europe reads the same digits differently. Index zero is empty: the rightmost group
    /// takes no name.
    /// </remarks>
    public static ReadOnlyCollection<string> Scales { get; } = new([
        string.Empty, "THOUSAND", "MILLION", "BILLION", "TRILLION", "QUADRILLION",
        "QUINTILLION", "SEXTILLION", "SEPTILLION", "OCTILLION",
    ]);

    /// <summary>The largest number of digits the scale names can describe.</summary>
    public static int MaxDigits => Scales.Count * DigitsPerGroup;

    /// <summary>The number of digits in one scale group.</summary>
    public const int DigitsPerGroup = 3;
}
