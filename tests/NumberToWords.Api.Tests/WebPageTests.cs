using System.Net;

namespace NumberToWords.Api.Tests;

public sealed class WebPageTests : IClassFixture<ConversionApiFactory>
{
    private readonly ConversionApiFactory _factory;

    public WebPageTests(ConversionApiFactory factory) => _factory = factory;

    [Fact]
    public async Task The_root_path_serves_the_converter_page()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(new Uri("/", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);

        string html = await response.Content.ReadAsStringAsync();
        Assert.Contains("id=\"amount\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_page_assets_are_served()
    {
        // A missing asset shows up in a browser only as an unstyled or dead page.
        using HttpClient client = _factory.CreateClient();

        foreach ((string path, string mediaType) in new[]
        {
            ("/styles.css", "text/css"), ("/app.js", "text/javascript"),
        })
        {
            using HttpResponseMessage response = await client.GetAsync(new Uri(path, UriKind.Relative));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(mediaType, response.Content.Headers.ContentType?.MediaType);
        }
    }

    [Fact]
    public async Task The_health_endpoint_reports_the_service_is_up()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(new Uri("/health", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Every_response_carries_the_security_headers()
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(new Uri("/", UriKind.Relative));

        foreach (string header in new[]
        {
            "Content-Security-Policy", "X-Content-Type-Options", "X-Frame-Options", "Referrer-Policy",
        })
        {
            Assert.True(response.Headers.Contains(header), $"The response is missing {header}.");
        }

        // Inline script would undo the main protection the page has against an injected DOM.
        string policy = string.Join(' ', response.Headers.GetValues("Content-Security-Policy"));
        Assert.Contains("default-src 'self'", policy, StringComparison.Ordinal);
        Assert.DoesNotContain("unsafe-inline", policy, StringComparison.Ordinal);
    }
}
