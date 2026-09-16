using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace NumberToWords.Api.Tests;

public sealed class ConversionEndpointTests : IClassFixture<ConversionApiFactory>
{
    private const string Endpoint = "/api/conversions";

    private readonly ConversionApiFactory _factory;

    public ConversionEndpointTests(ConversionApiFactory factory) => _factory = factory;

    [Theory]
    [InlineData("123.45", "ONE HUNDRED AND TWENTY-THREE DOLLARS AND FORTY-FIVE CENTS")]
    [InlineData("1,234,567.89", "ONE MILLION TWO HUNDRED AND THIRTY-FOUR THOUSAND FIVE HUNDRED AND SIXTY-SEVEN DOLLARS AND EIGHTY-NINE CENTS")]
    [InlineData("-99.99", "MINUS NINETY-NINE DOLLARS AND NINETY-NINE CENTS")]
    public async Task Post_converts_a_valid_amount(string value, string expectedWords)
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.PostAsJsonAsync(Endpoint, new { value });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(expectedWords, body.GetProperty("words").GetString());
        Assert.Equal(value, body.GetProperty("input").GetString());
    }

    [Theory]
    [InlineData("", "input.empty")]
    [InlineData("1.234", "input.too_many_decimal_places")]
    public async Task Post_refuses_bad_input_with_a_problem_document(string value, string expectedCode)
    {
        using HttpClient client = _factory.CreateClient();

        using HttpResponseMessage response = await client.PostAsJsonAsync(Endpoint, new { value });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        JsonElement problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(expectedCode, problem.GetProperty("code").GetString());
        Assert.Equal(400, problem.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("detail").GetString()));
    }

    [Fact]
    public async Task Post_refuses_a_body_that_is_not_json()
    {
        // This one found a real bug: BadHttpRequestException reached UseExceptionHandler and
        // turned an obvious caller mistake into a 500. A stack trace on the wire helps nobody
        // but an attacker, so check the body stays clean too.
        using HttpClient client = _factory.CreateClient();
        using StringContent content = new("this is not json", Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await client.PostAsync(new Uri(Endpoint, UriKind.Relative), content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Exception", body, StringComparison.OrdinalIgnoreCase);

        JsonElement problem = JsonDocument.Parse(body).RootElement;
        Assert.Equal("request.malformed", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Post_never_returns_a_server_error_for_hostile_input()
    {
        // Anything a user can type has to come back as a 400, never as a stack trace.
        string[] hostile = ["<script>alert(1)</script>", "\0", "NaN", "1e400", "٣٤٥", "'; DROP TABLE", new('1', 200)];
        using HttpClient client = _factory.CreateClient();

        foreach (string value in hostile)
        {
            using HttpResponseMessage response = await client.PostAsJsonAsync(Endpoint, new { value });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
