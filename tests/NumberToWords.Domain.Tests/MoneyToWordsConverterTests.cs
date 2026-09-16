using NumberToWords.Domain.Conversion;

namespace NumberToWords.Domain.Tests;

public sealed class MoneyToWordsConverterTests
{
    private readonly MoneyToWordsConverter _converter =
        new(new EnglishWholeNumberToWordsConverter(), CurrencyNames.Dollars);

    [Theory]
    [InlineData("123", 45, "ONE HUNDRED AND TWENTY-THREE DOLLARS AND FORTY-FIVE CENTS")]
    [InlineData("1", 1, "ONE DOLLAR AND ONE CENT")]
    [InlineData("0", 45, "FORTY-FIVE CENTS")]
    // Dropping both parts would leave an empty string, so zero keeps its whole unit.
    [InlineData("0", 0, "ZERO DOLLARS")]
    public void Convert_names_an_amount(string wholeUnits, int fractionalUnits, string expected) =>
        Assert.Equal(expected, _converter.Convert(Money.Create(false, wholeUnits, fractionalUnits)));

    [Fact]
    public void Convert_marks_an_amount_below_zero_with_minus() =>
        Assert.Equal("MINUS ONE CENT", _converter.Convert(Money.Create(true, "0", 1)));

    [Fact]
    public void Convert_uses_whichever_currency_words_it_was_given()
    {
        // Swapping the currency is why the unit names arrive as data at all.
        MoneyToWordsConverter sterling =
            new(new EnglishWholeNumberToWordsConverter(), CurrencyNames.PoundsSterling);

        Assert.Equal("ONE POUND AND FIFTY PENCE", sterling.Convert(Money.Create(false, "1", 50)));
    }
}
