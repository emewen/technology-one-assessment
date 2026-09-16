using Microsoft.AspNetCore.Mvc.Testing;

namespace NumberToWords.Api.Tests;

// Starts the real application in memory, so these tests run through the shipping pipeline:
// model binder, JSON serialiser, security headers and all.
public sealed class ConversionApiFactory : WebApplicationFactory<Program>
{
}
