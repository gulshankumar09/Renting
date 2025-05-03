using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using UserManagement.Models.Entities;
using UserManagement.Services;

namespace UserManagement.Tests.Services;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;

    public JwtTokenServiceTests()
    {
        // Setup configuration
        var configValues = new Dictionary<string, string?>
        {
            {"Jwt:Key", "supersecrettestkey_atleast32characters_long"},
            {"Jwt:Issuer", "test_issuer"},
            {"Jwt:Audience", "test_audience"},
            {"Jwt:TokenValidityInMinutes", "60"},
            {"Jwt:RefreshTokenValidityInDays", "7"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        _jwtTokenService = new JwtTokenService(_configuration);
    }

    [Fact]
    public void GenerateJwtToken_WithValidClaims_ShouldReturnToken()
    {
        // Arrange
        var testUser = new ApplicationUser
        {
            Id = "user123",
            UserName = "test@example.com",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var token = _jwtTokenService.GenerateJwtToken(testUser);

        // Assert
        token.Should().NotBeNullOrEmpty();

        // Verify the token can be decoded
        var tokenHandler = new JwtSecurityTokenHandler();
        tokenHandler.CanReadToken(token).Should().BeTrue();

        var jwtToken = tokenHandler.ReadJwtToken(token);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "user123");
        jwtToken.Claims.Should().Contain(c => c.Type == "email" && c.Value == "test@example.com");
        jwtToken.Claims.Should().Contain(c => c.Type == "name" && c.Value == "Test User");

        jwtToken.Issuer.Should().Be("test_issuer");
        jwtToken.Audiences.Should().Contain("test_audience");
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WithValidToken_ShouldReturnPrincipal()
    {
        // Arrange
        var testUser = new ApplicationUser
        {
            Id = "user123",
            UserName = "test@example.com",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        var token = _jwtTokenService.GenerateJwtToken(testUser);

        // Act
        var principal = _jwtTokenService.GetPrincipalFromExpiredToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be("user123");
        principal.FindFirst("email")?.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WithInvalidToken_ShouldThrowException()
    {
        // Arrange
        var invalidToken = "invalid.token.string";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _jwtTokenService.GetPrincipalFromExpiredToken(invalidToken));
    }
}