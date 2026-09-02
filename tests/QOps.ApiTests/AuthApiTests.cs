using System.Net;
using System.Net.Http.Json;
using QOps.Application.Users;

namespace QOps.ApiTests;

[Collection(QOpsApiCollection.Name)]
public class AuthApiTests(QOpsWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_ShouldReturnUserWithoutPassword()
    {
        var request = new RegisterUserRequest(
            $"user-{Guid.NewGuid():N}@example.com",
            "Strong-password-123!");

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(user);
        Assert.Equal(request.Email.ToLowerInvariant(), user.Email);
        Assert.True(user.IsActive);
    }

    [Fact]
    public async Task Login_ShouldReturnJwtForValidCredentials()
    {
        var email = $"login-{Guid.NewGuid():N}@example.com";
        var password = "Strong-password-123!";
        var registerResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterUserRequest(email, password));
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrWhiteSpace(auth.Token));
        Assert.Equal(email, auth.User.Email);
    }

    [Fact]
    public async Task Login_ShouldRejectInvalidPassword()
    {
        var email = $"invalid-{Guid.NewGuid():N}@example.com";
        var registerResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterUserRequest(email, "Strong-password-123!"));
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, "wrong-password"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
