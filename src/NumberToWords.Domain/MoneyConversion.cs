namespace NumberToWords.Domain;

/// <summary>
/// A completed conversion, holding enough context for a caller to show its working.
/// </summary>
/// <param name="Input">The text as the person typed it.</param>
/// <param name="Amount">
/// The amount the parser read, in digits. Showing it back lets someone confirm that
/// "1,234.5" was understood as 1234.50 before they trust the words.
/// </param>
/// <param name="Words">The amount in words.</param>
public sealed record MoneyConversion(string Input, string Amount, string Words);
