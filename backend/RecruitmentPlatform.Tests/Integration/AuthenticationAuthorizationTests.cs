using Xunit;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace RecruitmentPlatform.Tests.Integration;

public sealed class AuthenticationAuthorizationTests
    : IClassFixture<
        RecruitmentApiFactory>
{
    private readonly HttpClient _client;

    public AuthenticationAuthorizationTests(
        RecruitmentApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UsersWithoutToken_ReturnsUnauthorized()
    {
        var response =
            await _client.GetAsync(
                "/api/Users");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task CandidateToken_CannotAccessAdministratorUsers()
    {
        var token =
            await RegisterCandidate();

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "/api/Users");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var response =
            await _client.SendAsync(
                request);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task DuplicateRegistration_ReturnsConflict()
    {
        var email =
            $"candidate-{Guid.NewGuid():N}@example.com";

        var payload =
            new
            {
                firstName = "Test",
                lastName = "Candidate",
                email,
                password =
                    "Candidate123"
            };

        var first =
            await _client.PostAsJsonAsync(
                "/api/Auth/register",
                payload);

        var second =
            await _client.PostAsJsonAsync(
                "/api/Auth/register",
                payload);

        Assert.Equal(
            HttpStatusCode.OK,
            first.StatusCode);

        Assert.Equal(
            HttpStatusCode.Conflict,
            second.StatusCode);
    }

    [Fact]
    public async Task InvalidRegistration_ReturnsBadRequest()
    {
        var response =
            await _client.PostAsJsonAsync(
                "/api/Auth/register",
                new
                {
                    firstName = "1",
                    lastName = "",
                    email = "bad-email",
                    password = "short"
                });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    private async Task<string>
        RegisterCandidate()
    {
        var response =
            await _client.PostAsJsonAsync(
                "/api/Auth/register",
                new
                {
                    firstName = "Test",
                    lastName =
                        "Candidate",
                    email =
                        $"candidate-{Guid.NewGuid():N}@example.com",
                    password =
                        "Candidate123"
                });

        response.EnsureSuccessStatusCode();

        using var json =
            JsonDocument.Parse(
                await response.Content
                    .ReadAsStringAsync());

        return json.RootElement
            .GetProperty("token")
            .GetString()
            ?? throw new
                InvalidOperationException(
                    "JWT token was not returned.");
    }
}
