using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using NumberToWords.Api.Endpoints;
using NumberToWords.Api.Infrastructure;
using NumberToWords.Domain.Conversion;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddNumberToWords(CurrencyNames.Dollars);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHealthChecks();

// App Service terminates TLS at its front end. Without these headers the application sees
// neither the real scheme nor the client address.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// The conversion is cheap. This protects the free tier's daily quota from one noisy
// caller, nothing more.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 120,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));
});

WebApplication app = builder.Build();

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseSecurityHeaders();

if (!app.Environment.IsDevelopment())
{
    // Development runs over plain HTTP on localhost, where these only get in the way.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRateLimiter();

app.MapConversionEndpoints();
app.MapHealthChecks("/health");

await app.RunAsync().ConfigureAwait(false);

#pragma warning disable CA1515 // The entry point is public because the test host requires it.

/// <summary>
/// Named so the integration tests can start this application in memory.
/// </summary>
/// <remarks>
/// Top level statements compile into an internal Program class that
/// <c>WebApplicationFactory&lt;T&gt;</c> cannot reach from a test assembly. xUnit also
/// requires public test classes, and a public class cannot close a generic over a less
/// accessible type. Hence one public type in an otherwise internal assembly.
/// </remarks>
public sealed partial class Program
{
    private Program()
    {
    }
}

#pragma warning restore CA1515
