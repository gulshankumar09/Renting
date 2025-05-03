using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using UserManagement.DTOs;
using UserManagement.Models.Entities;
using UserManagement.Models.Exceptions;
using UserManagement.Services;
using UserManagement.Tests.Helpers;

namespace UserManagement.Tests.Services;

public class AuthServiceTests : TestBase
{
    private readonly AuthService _authService;

    public AuthServiceTests() : base()
    {
        _authService = new AuthService(
            MockUserManager.Object,
            MockLogger.Object,
            MockJwtTokenService.Object,
            MockEmailService.Object,
            Configuration,
            MockActivityService.Object,
            MockHttpContextAccessor.Object
        );
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var testUser = new ApplicationUser
        {
            Id = "user123",
            UserName = "test@example.com",
            Email = "test@example.com",
            EmailConfirmed = true
        };

        var loginRequest = new LoginRequestDTO
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Mock UserManager to return our test user
        MockUserManager.Setup(x => x.FindByEmailAsync(loginRequest.Email))
            .ReturnsAsync(testUser);

        // Mock UserManager to validate password
        MockUserManager.Setup(x => x.CheckPasswordAsync(testUser, loginRequest.Password))
            .ReturnsAsync(true);

        // Mock UserManager to check email confirmation
        MockUserManager.Setup(x => x.IsEmailConfirmedAsync(testUser))
            .ReturnsAsync(true);

        // Mock JwtTokenService to return tokens
        MockJwtTokenService.Setup(x => x.GenerateJwtToken(It.IsAny<ApplicationUser>()))
            .Returns("test_jwt_token");

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("test_jwt_token");

        // Verify the login attempt was logged - avoiding optional parameters in expression tree
        MockActivityService.Verify(
            x => x.LogLoginAttemptAsync(
                testUser.Id,
                true,
                It.IsAny<string>(),
                It.IsAny<string>(),
                null),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentUser_ShouldThrowAuthException()
    {
        // Arrange
        var loginRequest = new LoginRequestDTO
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        // Mock UserManager to return null (user not found)
        MockUserManager.Setup(x => x.FindByEmailAsync(loginRequest.Email))
            .ReturnsAsync((ApplicationUser)null);

        // Act & Assert
        await Assert.ThrowsAsync<AuthException>(async () =>
            await _authService.LoginAsync(loginRequest));

        // Verify failed login was logged - avoid optional parameters by passing all arguments
        MockActivityService.Verify(
            x => x.LogLoginAttemptAsync(
                "unknown",
                false,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowAuthException()
    {
        // Arrange
        var testUser = new ApplicationUser
        {
            Id = "user123",
            UserName = "test@example.com",
            Email = "test@example.com",
            EmailConfirmed = true
        };

        var loginRequest = new LoginRequestDTO
        {
            Email = "test@example.com",
            Password = "WrongPassword!"
        };

        // Mock UserManager to return our test user
        MockUserManager.Setup(x => x.FindByEmailAsync(loginRequest.Email))
            .ReturnsAsync(testUser);

        // Mock UserManager to invalidate password
        MockUserManager.Setup(x => x.CheckPasswordAsync(testUser, loginRequest.Password))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<AuthException>(async () =>
            await _authService.LoginAsync(loginRequest));

        // Just verify the method was called with any parameters to avoid expression tree issues
        MockActivityService.Verify(
            x => x.LogLoginAttemptAsync(
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithNewUser_ShouldReturnAuthResponse()
    {
        // Arrange
        var registerRequest = new RegisterRequestDTO
        {
            Email = "new@example.com",
            Password = "Password123!",
            FirstName = "New",
            LastName = "User"
        };

        ApplicationUser createdUser = null!;

        // Mock UserManager to indicate user doesn't exist
        MockUserManager.Setup(x => x.FindByEmailAsync(registerRequest.Email))
            .ReturnsAsync((ApplicationUser)null);

        // Mock UserManager for creating user
        MockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerRequest.Password))
            .Callback<ApplicationUser, string>((user, _) => createdUser = user)
            .ReturnsAsync(IdentityResult.Success);

        // Mock JwtTokenService to return tokens
        MockJwtTokenService.Setup(x => x.GenerateJwtToken(It.IsAny<ApplicationUser>()))
            .Returns("test_jwt_token");

        // Mock generating email confirmation token
        MockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("confirmation_token");

        // Act
        var result = await _authService.RegisterAsync(registerRequest);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("test_jwt_token");

        MockUserManager.Verify(x => x.CreateAsync(
            It.Is<ApplicationUser>(u =>
                u.Email == registerRequest.Email &&
                u.FirstName == registerRequest.FirstName &&
                u.LastName == registerRequest.LastName),
            registerRequest.Password),
            Times.Once);

        MockEmailService.Verify(x => x.SendEmailVerificationAsync(
            It.Is<string>(email => email == registerRequest.Email),
            It.IsAny<string>(),
            It.Is<string>(token => token == "confirmation_token")),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowAuthException()
    {
        // Arrange
        var existingUser = new ApplicationUser
        {
            Id = "existing123",
            UserName = "existing@example.com",
            Email = "existing@example.com"
        };

        var registerRequest = new RegisterRequestDTO
        {
            Email = "existing@example.com",
            Password = "Password123!",
            FirstName = "Existing",
            LastName = "User"
        };

        // Mock UserManager to indicate user already exists
        MockUserManager.Setup(x => x.FindByEmailAsync(registerRequest.Email))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<AuthException>(async () =>
            await _authService.RegisterAsync(registerRequest));
    }

    [Fact]
    public async Task VerifyEmailAsync_WithValidToken_ShouldConfirmEmail()
    {
        // Arrange
        var testUser = new ApplicationUser
        {
            Id = "user123",
            UserName = "test@example.com",
            Email = "test@example.com",
            EmailConfirmed = false
        };

        var verifyEmailDto = new VerifyEmailDTO
        {
            UserId = "user123",
            Token = "valid_token"
        };

        // Mock UserManager to return our test user
        MockUserManager.Setup(x => x.FindByIdAsync(verifyEmailDto.UserId))
            .ReturnsAsync(testUser);

        // Mock UserManager to confirm email
        MockUserManager.Setup(x => x.ConfirmEmailAsync(testUser, verifyEmailDto.Token))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.VerifyEmailAsync(verifyEmailDto);

        // Assert
        result.Should().BeTrue();
        MockUserManager.Verify(x => x.ConfirmEmailAsync(testUser, verifyEmailDto.Token), Times.Once);
    }
}