namespace NumberToWords.Domain;

/// <summary>
/// A reason that user supplied input was refused.
/// </summary>
/// <param name="Code">
/// A stable identifier for the failure. Callers switch on this and never on
/// <paramref name="Message"/>, so the wording can change without breaking them.
/// </param>
/// <param name="Message">Text written for the person who typed the input.</param>
public sealed record ValidationError(string Code, string Message)
{
    /// <summary>The caller supplied nothing to convert.</summary>
    public static ValidationError Empty { get; } = new(
        "input.empty",
        "Enter an amount to convert.");

    /// <summary>The input ran past the length this service accepts.</summary>
    public static ValidationError TooLong(int maximumLength) => new(
        "input.too_long",
        $"Enter no more than {maximumLength} characters.");

    /// <summary>The input holds a character that cannot appear in an amount.</summary>
    public static ValidationError InvalidCharacter(char character) => new(
        "input.invalid_character",
        $"'{character}' is not valid in an amount. Use digits, one decimal point, and an optional leading minus sign.");

    /// <summary>The input holds more than one decimal point.</summary>
    public static ValidationError MultipleDecimalPoints { get; } = new(
        "input.multiple_decimal_points",
        "An amount can hold only one decimal point.");

    /// <summary>The input has a fraction finer than the smallest coin.</summary>
    public static ValidationError TooManyDecimalPlaces { get; } = new(
        "input.too_many_decimal_places",
        "Enter at most two decimal places. Amounts are not rounded for you.");

    /// <summary>The input held punctuation or a sign but no digits.</summary>
    public static ValidationError NoDigits { get; } = new(
        "input.no_digits",
        "Enter at least one digit.");

    /// <summary>The whole part of the amount is larger than the largest name available.</summary>
    public static ValidationError TooLarge(int maximumDigits) => new(
        "value.too_large",
        $"The amount before the decimal point can hold at most {maximumDigits} digits.");
}
