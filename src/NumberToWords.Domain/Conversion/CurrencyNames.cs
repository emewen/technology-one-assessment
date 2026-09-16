namespace NumberToWords.Domain.Conversion;

/// <summary>
/// The words a currency uses for its whole and fractional units.
/// </summary>
/// <remarks>
/// These live as data so that adding a currency means writing a new record and no new
/// conversion code.
/// </remarks>
/// <param name="WholeUnitSingular">The whole unit when there is exactly one, such as "DOLLAR".</param>
/// <param name="WholeUnitPlural">The whole unit for any other count, such as "DOLLARS".</param>
/// <param name="FractionalUnitSingular">The fractional unit when there is exactly one, such as "CENT".</param>
/// <param name="FractionalUnitPlural">The fractional unit for any other count, such as "CENTS".</param>
public sealed record CurrencyNames(
    string WholeUnitSingular,
    string WholeUnitPlural,
    string FractionalUnitSingular,
    string FractionalUnitPlural)
{
    /// <summary>Dollars and cents.</summary>
    public static CurrencyNames Dollars { get; } = new("DOLLAR", "DOLLARS", "CENT", "CENTS");

    /// <summary>Pounds and pence. A worked example of a second currency.</summary>
    public static CurrencyNames PoundsSterling { get; } = new("POUND", "POUNDS", "PENNY", "PENCE");

    /// <summary>Returns the whole unit name that agrees with <paramref name="count"/>.</summary>
    /// <param name="count">The number of whole units, as digits.</param>
    /// <exception cref="ArgumentNullException"><paramref name="count"/> is null.</exception>
    public string WholeUnitFor(string count)
    {
        ArgumentNullException.ThrowIfNull(count);
        return count == "1" ? WholeUnitSingular : WholeUnitPlural;
    }

    /// <summary>Returns the fractional unit name that agrees with <paramref name="count"/>.</summary>
    /// <param name="count">The number of fractional units.</param>
    public string FractionalUnitFor(int count) =>
        count == 1 ? FractionalUnitSingular : FractionalUnitPlural;
}
