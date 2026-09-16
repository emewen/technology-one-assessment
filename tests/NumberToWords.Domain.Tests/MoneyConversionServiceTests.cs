using NumberToWords.Domain.Conversion;
using NumberToWords.Domain.Parsing;

namespace NumberToWords.Domain.Tests;

public sealed class MoneyConversionServiceTests
{
    private readonly MoneyConversionService _service = new(
        new MoneyParser(),
        new MoneyToWordsConverter(new EnglishWholeNumberToWordsConverter(), CurrencyNames.Dollars));

    [Fact]
    public void Convert_parses_and_names_in_one_call()
    {
        // The echoed amount lets someone confirm "1,234.5" was read as 1234.50 before they
        // trust the words.
        Result<MoneyConversion> result = _service.Convert("1,234.5");

        Assert.True(result.IsSuccess);
        Assert.Equal("1,234.5", result.Value.Input);
        Assert.Equal("1234.50", result.Value.Amount);
        Assert.Equal("ONE THOUSAND TWO HUNDRED AND THIRTY-FOUR DOLLARS AND FIFTY CENTS", result.Value.Words);
    }

    [Fact]
    public void Convert_passes_the_parser_failure_straight_through()
    {
        Result<MoneyConversion> result = _service.Convert("1.234");

        Assert.False(result.IsSuccess);
        Assert.Equal("input.too_many_decimal_places", result.Error.Code);
    }
}
