using NumberToWords.Domain.Conversion;
using NumberToWords.Domain.Parsing;

namespace NumberToWords.Domain;

/// <summary>
/// Runs the parser and the converter in order and reports whichever step failed.
/// </summary>
public sealed class MoneyConversionService : IMoneyConversionService
{
    private readonly IMoneyParser _parser;
    private readonly IMoneyToWordsConverter _converter;

    /// <summary>
    /// Creates the service.
    /// </summary>
    /// <param name="parser">Reads text into an amount.</param>
    /// <param name="converter">Names an amount.</param>
    /// <exception cref="ArgumentNullException">Either argument is null.</exception>
    public MoneyConversionService(IMoneyParser parser, IMoneyToWordsConverter converter)
    {
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(converter);

        _parser = parser;
        _converter = converter;
    }

    /// <inheritdoc />
    public int MaxInputLength => _parser.MaxInputLength;

    /// <inheritdoc />
    public Result<MoneyConversion> Convert(string? input)
    {
        Result<Money> parsed = _parser.Parse(input);
        if (!parsed.IsSuccess)
        {
            return Result<MoneyConversion>.Failure(parsed.Error);
        }

        Money money = parsed.Value;
        MoneyConversion conversion = new(
            Input: input ?? string.Empty,
            Amount: money.ToString(),
            Words: _converter.Convert(money));

        return Result<MoneyConversion>.Success(conversion);
    }
}
