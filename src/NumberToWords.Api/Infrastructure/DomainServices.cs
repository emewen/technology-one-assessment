using NumberToWords.Domain;
using NumberToWords.Domain.Conversion;
using NumberToWords.Domain.Parsing;

namespace NumberToWords.Api.Infrastructure;

/// <summary>
/// Registers the domain with the container.
/// </summary>
internal static class DomainServices
{
    /// <summary>
    /// Adds the parser, the converters and the service that composes them.
    /// </summary>
    /// <remarks>
    /// None of these hold mutable state, so they register as singletons. The application
    /// allocates one of each for its lifetime.
    /// </remarks>
    /// <param name="services">The container to add to.</param>
    /// <param name="currency">The currency words to convert into.</param>
    /// <returns>The same container, for chaining.</returns>
    /// <exception cref="ArgumentNullException">Either argument is null.</exception>
    public static IServiceCollection AddNumberToWords(this IServiceCollection services, CurrencyNames currency)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(currency);

        services.AddSingleton(currency);
        services.AddSingleton<IWholeNumberToWordsConverter, EnglishWholeNumberToWordsConverter>();
        services.AddSingleton<IMoneyParser, MoneyParser>();
        services.AddSingleton<IMoneyToWordsConverter, MoneyToWordsConverter>();
        services.AddSingleton<IMoneyConversionService, MoneyConversionService>();

        return services;
    }
}
