using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using RepoPoc.Api.Dtos;

namespace RepoPoc.Api.Tests;

public class UsersControllerTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public UsersControllerTests(ApiFactory factory)
    {
        _factory = factory;
    }

    private async Task<(HttpClient Client, LoginResponse Login)> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(ApiFactory.AdminUsername, ApiFactory.AdminPassword));
        response.EnsureSuccessStatusCode();
        var login = (await response.Content.ReadFromJsonAsync<LoginResponse>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        return (client, login);
    }

    private static CreateUserRequest NewUser(string suffix) => new()
    {
        Username = $"user{suffix}",
        Password = "Secret123!",
        FirstName = "John",
        LastName = "Doe",
        Email = $"user{suffix}@example.com",
        Phone = "600000000"
    };

    [Fact]
    public async Task GetAll_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Crud_Flow_Works()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        // Create
        var createResponse = await client.PostAsJsonAsync("/api/users", NewUser(suffix));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = (await createResponse.Content.ReadFromJsonAsync<UserDto>())!;
        Assert.Equal($"user{suffix}", created.Username);

        // Read
        var users = await client.GetFromJsonAsync<List<UserDto>>("/api/users");
        Assert.Contains(users!, u => u.Id == created.Id);
        var fetched = await client.GetFromJsonAsync<UserDto>($"/api/users/{created.Id}");
        Assert.Equal(created.Email, fetched!.Email);

        // Update
        var updateResponse = await client.PutAsJsonAsync($"/api/users/{created.Id}", new UpdateUserRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = $"jane{suffix}@example.com",
            Password = "NewSecret123!"
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = (await updateResponse.Content.ReadFromJsonAsync<UserDto>())!;
        Assert.Equal("Jane", updated.FirstName);
        Assert.NotNull(updated.UpdatedAt);

        // The new user can log in with the updated password
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest($"user{suffix}", "NewSecret123!"));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Delete
        var deleteResponse = await client.DeleteAsync($"/api/users/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        var getDeleted = await client.GetAsync($"/api/users/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getDeleted.StatusCode);
    }

    [Fact]
    public async Task Create_WithDuplicateUsername_ReturnsConflict()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        (await client.PostAsJsonAsync("/api/users", NewUser(suffix))).EnsureSuccessStatusCode();
        var duplicate = NewUser(suffix);
        duplicate.Email = $"other{suffix}@example.com";

        var response = await client.PostAsJsonAsync("/api/users", duplicate);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithInvalidData_ReturnsBadRequest()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var invalid = NewUser("x");
        invalid.Email = "not-an-email";
        invalid.Password = "123";

        var response = await client.PostAsJsonAsync("/api/users", invalid);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_And_Delete_NonExistingUser_ReturnNotFound()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var update = await client.PutAsJsonAsync("/api/users/999999", new UpdateUserRequest
        {
            FirstName = "A",
            LastName = "B",
            Email = "nobody@example.com"
        });
        var delete = await client.DeleteAsync("/api/users/999999");

        Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
    }

    [Fact]
    public async Task Delete_CurrentUser_ReturnsBadRequest()
    {
        var (client, login) = await CreateAuthenticatedClientAsync();

        var response = await client.DeleteAsync($"/api/users/{login.User.Id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
