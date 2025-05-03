using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UserManagement.DTOs;
using UserManagement.Tests.Helpers;

namespace UserManagement.Tests.AuthFlows;

public class AuthenticationFlowTests : IClassFixture<TestWebApplicationFactory<object>>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory<object> _factory;

    public AuthenticationFlowTests(TestWebApplicationFactory<object> factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task CompleteAuthFlow_RegisterLoginChangePasswordLogout_ShouldSucceed()
    {
        // Step 1: Register a new user
        var email = $"flowuser_{Guid.NewGuid()}@example.com";
        var password = "Test@123";
        var newPassword = "NewPassword@123";

        var registerRequest = new RegisterRequestDTO
        {
            Email = email,
            Password = password,
            FirstName = "Flow",
            LastName = "User"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/Auth/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        registerContent.Should().Contain("token");

        // Step 2: Login with the newly created user
        var loginRequest = new LoginRequestDTO
        {
            Email = email,
            Password = password
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        loginContent.Should().Contain("token");
        loginContent.Should().Contain("refreshToken");

        // Get the token for authenticated requests
        var token = await AuthHelper.GetJwtTokenAsync(_client, email, password);
        AuthHelper.AuthenticateClient(_client, token);

        // Step 3: Change password
        var changePasswordRequest = new ChangePasswordRequestDTO
        {
            CurrentPassword = password,
            NewPassword = newPassword
        };

        var changePasswordResponse = await _client.PostAsJsonAsync("/api/Auth/change-password", changePasswordRequest);
        changePasswordResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 4: Verify login with new password works
        _client.DefaultRequestHeaders.Authorization = null; // Clear auth header
        var newLoginRequest = new LoginRequestDTO
        {
            Email = email,
            Password = newPassword
        };

        var newLoginResponse = await _client.PostAsJsonAsync("/api/Auth/login", newLoginRequest);
        newLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Get new token
        token = await AuthHelper.GetJwtTokenAsync(_client, email, newPassword);
        AuthHelper.AuthenticateClient(_client, token);

        // Step 5: Revoke token (logout)
        var revokeResponse = await _client.PostAsync("/api/Auth/revoke-token", null);
        revokeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 6: Verify protected endpoint access is denied after logout
        // First, try with the revoked token (should fail)
        var protectedResponse = await _client.GetAsync("/api/UserProfile");
        protectedResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshTokenFlow_LoginRefreshAccessProtected_ShouldSucceed()
    {
        // Step 1: Login to get initial tokens
        var loginRequest = new LoginRequestDTO
        {
            Email = "testuser@example.com",
            Password = "Test@123"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
        var authResponse = System.Text.Json.JsonSerializer.Deserialize<AuthResponseDTO>(
            loginResponseContent,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.RefreshToken.Should().NotBeNullOrEmpty();

        // Step 2: Use the token to access a protected resource
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse.Token);

        var initialProtectedResponse = await _client.GetAsync("/api/UserProfile");
        initialProtectedResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 3: Use refresh token to get a new access token
        var refreshRequest = new RefreshTokenRequestDTO
        {
            Token = authResponse.Token,
            RefreshToken = authResponse.RefreshToken
        };

        var refreshResponse = await _client.PostAsJsonAsync("/api/Auth/refresh-token", refreshRequest);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshResponseContent = await refreshResponse.Content.ReadAsStringAsync();
        var newAuthResponse = System.Text.Json.JsonSerializer.Deserialize<AuthResponseDTO>(
            refreshResponseContent,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        newAuthResponse.Should().NotBeNull();
        newAuthResponse!.Token.Should().NotBeNullOrEmpty();
        newAuthResponse.Token.Should().NotBe(authResponse.Token);

        // Step 4: Use the new token to access a protected resource
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newAuthResponse.Token);

        var finalProtectedResponse = await _client.GetAsync("/api/UserProfile");
        finalProtectedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}