using NumberToWords.Domain.Conversion;

namespace NumberToWords.Domain.Tests;

public sealed class EnglishWholeNumberToWordsConverterTests
{
    private readonly EnglishWholeNumberToWordsConverter _converter = new();

    [Theory]
    [InlineData("0", "ZERO")]
    [InlineData("45", "FORTY-FIVE")]
    [InlineData("100", "ONE HUNDRED")]
    [InlineData("101", "ONE HUNDRED AND ONE")]
    [InlineData("007", "SEVEN")]
    [InlineData("1234567", "ONE MILLION TWO HUNDRED AND THIRTY-FOUR THOUSAND FIVE HUNDRED AND SIXTY-SEVEN")]
    public void Convert_names_a_whole_number(string digits, string expected) =>
        Assert.Equal(expected, _converter.Convert(digits));

    // The "AND" before a trailing remainder is the rule that is easiest to get wrong:
    // it appears under a hundred and stops at one.
    [Theory]
    [InlineData("1001", "ONE THOUSAND AND ONE")]
    [InlineData("1100", "ONE THOUSAND ONE HUNDRED")]
    public void Convert_places_and_before_a_remainder_under_a_hundred(string digits, string expected) =>
        Assert.Equal(expected, _converter.Convert(digits));

    [Fact]
    public void Convert_rejects_anything_that_is_not_a_digit() =>
        Assert.Throws<ArgumentException>(() => _converter.Convert("12a"));
}
