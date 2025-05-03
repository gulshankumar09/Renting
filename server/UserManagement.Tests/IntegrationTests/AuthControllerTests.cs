using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UserManagement.DTOs;
using UserManagement.Tests.Helpers;

namespace UserManagement.Tests.IntegrationTests;

public class AuthControllerTests : IClassFixture<TestWebApplicationFactory<object>>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory<object> _factory;

    public AuthControllerTests(TestWebApplicationFactory<object> factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOk()
    {
        // Arrange
        var loginRequest = new LoginRequestDTO
        {
            Email = "testuser@example.com",
            Password = "Test@123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("token");
        content.Should().Contain("refreshToken");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequestDTO
        {
            Email = "testuser@example.com",
            Password = "WrongPassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var registerRequest = new RegisterRequestDTO
        {
            Email = $"newuser_{Guid.NewGuid()}@example.com",
            Password = "Test@123",
            FirstName = "New",
            LastName = "User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/register", registerRequest);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        content.Should().Contain("token");
        content.Should().Contain("refreshToken");
    }

    [Fact]
    public async Task Register_WithExistingEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var registerRequest = new RegisterRequestDTO
        {
            Email = "testuser@example.com", // Already exists in TestWebApplicationFactory
            Password = "Test@123",
            FirstName = "Duplicate",
            LastName = "User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePassword_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new ChangePasswordRequestDTO
        {
            CurrentPassword = "Test@123",
            NewPassword = "NewPassword@123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_WithAuth_ShouldReturnOk()
    {
        // Arrange
        var token = await AuthHelper.GetJwtTokenAsync(_client, "testuser@example.com", "Test@123");
        AuthHelper.AuthenticateClient(_client, token);

        var request = new ChangePasswordRequestDTO
        {
            CurrentPassword = "Test@123",
            NewPassword = "NewPassword@123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Cleanup - Reset password for other tests
        var resetRequest = new ChangePasswordRequestDTO
        {
            CurrentPassword = "NewPassword@123",
            NewPassword = "Test@123"
        };
        await _client.PostAsJsonAsync("/api/Auth/change-password", resetRequest);
    }
}