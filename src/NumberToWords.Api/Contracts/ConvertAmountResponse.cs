namespace NumberToWords.Api.Contracts;

/// <summary>
/// A successful conversion.
/// </summary>
/// <param name="Input">The amount as the person typed it.</param>
/// <param name="Amount">The amount the service read, in digits.</param>
/// <param name="Words">The amount in words.</param>
internal sealed record ConvertAmountResponse(string Input, string Amount, string Words);
