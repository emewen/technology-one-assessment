namespace NumberToWords.Api.Infrastructure;

/// <summary>
/// Adds the response headers a browser needs to lock this page down.
/// </summary>
internal static class SecurityHeaders
{
    /// <summary>
    /// The content security policy for the page.
    /// </summary>
    /// <remarks>
    /// Everything the page loads comes from this origin, so the policy permits nothing else.
    /// That rules out inline script and inline style as a side effect. Hence the separate
    /// stylesheet and script files.
    /// </remarks>
    private const string ContentSecurityPolicy =
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self'; " +
        "img-src 'self' data:; " +
        "connect-src 'self'; " +
        "base-uri 'none'; " +
        "form-action 'none'; " +
        "frame-ancestors 'none'";

    /// <summary>
    /// Sets the security headers on every response.
    /// </summary>
    /// <param name="application">The pipeline to add to.</param>
    /// <returns>The same pipeline, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="application"/> is null.</exception>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder application)
    {
        ArgumentNullException.ThrowIfNull(application);

        return application.Use(async (context, next) =>
        {
            IHeaderDictionary headers = context.Response.Headers;

            headers["Content-Security-Policy"] = ContentSecurityPolicy;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), interest-cohort=()";

            await next().ConfigureAwait(false);
        });
    }
}
