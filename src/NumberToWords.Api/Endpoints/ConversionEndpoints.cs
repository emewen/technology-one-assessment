using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NumberToWords.Api.Contracts;
using NumberToWords.Api.Infrastructure;
using NumberToWords.Domain;

namespace NumberToWords.Api.Endpoints;

/// <summary>
/// The HTTP surface for converting an amount.
/// </summary>
internal static class ConversionEndpoints
{
    /// <summary>
    /// The log category for conversion requests.
    /// </summary>
    /// <remarks>
    /// A static class cannot be a generic type argument, so this closes
    /// <c>ILogger&lt;T&gt;</c> with an explicit category name instead.
    /// </remarks>
    private const string LogCategory = "NumberToWords.Api.Conversions";

    /// <summary>
    /// Maps the conversion endpoints onto <paramref name="routes"/>.
    /// </summary>
    /// <param name="routes">The route builder to map onto.</param>
    /// <returns>The group holding the mapped endpoints.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routes"/> is null.</exception>
    public static RouteGroupBuilder MapConversionEndpoints(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        RouteGroupBuilder group = routes.MapGroup("/api");

        group.MapPost("/conversions", ConvertAmount)
            .WithName("ConvertAmount")
            .Produces<ConvertAmountResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }

    /// <summary>
    /// Converts the amount in the request body into words.
    /// </summary>
    /// <remarks>
    /// A refused amount returns 400 with a problem document. The failure code goes in the
    /// "code" extension, so a client branches on the reason and never on the wording of the
    /// message.
    /// </remarks>
    private static Results<Ok<ConvertAmountResponse>, ProblemHttpResult> ConvertAmount(
        [FromBody] ConvertAmountRequest? request,
        IMoneyConversionService conversionService,
        ILoggerFactory loggerFactory)
    {
        ILogger logger = loggerFactory.CreateLogger(LogCategory);
        Result<MoneyConversion> result = conversionService.Convert(request?.Value);

        if (!result.IsSuccess)
        {
            // A mistyped amount is expected traffic. Information, not a warning.
            ConversionLog.ConversionRefused(logger, result.Error.Code);

            return TypedResults.Problem(
                detail: result.Error.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "The amount could not be read.",
                extensions: new Dictionary<string, object?> { ["code"] = result.Error.Code });
        }

        MoneyConversion conversion = result.Value;
        ConversionLog.Converted(logger, conversion.Amount);

        return TypedResults.Ok(new ConvertAmountResponse(
            Input: conversion.Input,
            Amount: conversion.Amount,
            Words: conversion.Words));
    }
}
