using NumberToWords.Domain.Parsing;

namespace NumberToWords.Domain.Tests;

public sealed class MoneyParserTests
{
    private readonly MoneyParser _parser = new();

    [Theory]
    [InlineData("123.45", "123", 45)]
    [InlineData("$1,234.56", "1234", 56)]
    // "0.5" is fifty cents. Pad on the left and it becomes five, which still looks plausible.
    [InlineData("0.5", "0", 50)]
    public void Parse_reads_an_amount(string input, string wholeUnits, int fractionalUnits)
    {
        Result<Money> result = _parser.Parse(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(wholeUnits, result.Value.WholeUnits);
        Assert.Equal(fractionalUnits, result.Value.FractionalUnits);
    }

    [Fact]
    public void Parse_reads_a_negative_amount()
    {
        Result<Money> result = _parser.Parse("-$5.00");

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsNegative);
        Assert.Equal("5", result.Value.WholeUnits);
    }

    // Every code here is part of the API contract, so each one gets a case.
    [Theory]
    [InlineData("", "input.empty")]
    [InlineData("abc", "input.invalid_character")]
    [InlineData("1.2.3", "input.multiple_decimal_points")]
    [InlineData("1.234", "input.too_many_decimal_places")]
    [InlineData("-", "input.no_digits")]
    public void Parse_reports_why_it_refused(string input, string expectedCode)
    {
        Result<Money> result = _parser.Parse(input);

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedCode, result.Error.Code);
        Assert.NotEmpty(result.Error.Message);
    }

    [Fact]
    public void Parse_refuses_a_whole_part_it_cannot_name()
    {
        Result<Money> result = _parser.Parse(new string('9', Money.MaxWholeUnitDigits + 1));

        Assert.False(result.IsSuccess);
        Assert.Equal("value.too_large", result.Error.Code);
    }
}
