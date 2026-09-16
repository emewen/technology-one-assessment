namespace NumberToWords.Domain.Tests;

public sealed class MoneyTests
{
    [Fact]
    public void Create_strips_leading_zeros_so_equal_amounts_compare_equal()
    {
        Money padded = Money.Create(false, "00123", 45);

        Assert.Equal("123", padded.WholeUnits);
        Assert.Equal(Money.Create(false, "123", 45), padded);
    }

    [Fact]
    public void Create_drops_the_sign_on_an_amount_of_nothing()
    {
        // Nobody says "MINUS ZERO DOLLARS". The sign still survives on -0.01.
        Assert.False(Money.Create(isNegative: true, "0", 0).IsNegative);
        Assert.True(Money.Create(isNegative: true, "0", 1).IsNegative);
    }

    [Fact]
    public void ToString_pads_the_fractional_part_so_123_05_does_not_render_as_123_5() =>
        Assert.Equal("123.05", Money.Create(false, "123", 5).ToString());

    [Fact]
    public void Create_rejects_a_whole_part_longer_than_the_converter_can_name() =>
        Assert.Throws<ArgumentException>(
            () => Money.Create(false, new string('9', Money.MaxWholeUnitDigits + 1), 0));
}
