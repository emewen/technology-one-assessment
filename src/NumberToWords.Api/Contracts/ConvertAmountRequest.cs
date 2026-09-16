namespace NumberToWords.Api.Contracts;

/// <summary>
/// The body of a conversion request.
/// </summary>
/// <param name="Value">
/// The amount as the user typed it. It stays a string so the service sees the input
/// character for character. Binding it to a decimal would hand the framework's parser the
/// exact decisions this service exists to make: how many decimal places to accept, what to
/// do with input it cannot read.
/// </param>
internal sealed record ConvertAmountRequest(string? Value);
