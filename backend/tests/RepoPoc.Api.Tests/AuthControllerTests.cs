using System.Net;
using System.Net.Http.Json;
using RepoPoc.Api.Dtos;

namespace RepoPoc.Api.Tests;

public class AuthControllerTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(ApiFactory.AdminUsername, ApiFactory.AdminPassword));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.Token));
        Assert.Equal(ApiFactory.AdminUsername, body.User.Username);
    }

    [Theory]
    [InlineData(ApiFactory.AdminUsername, "wrong-password")]
    [InlineData("unknown", ApiFactory.AdminPassword)]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized(string username, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(username, password));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithMissingFields_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { username = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
