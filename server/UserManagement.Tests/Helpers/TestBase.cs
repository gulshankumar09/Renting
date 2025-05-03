using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UserManagement.Data;
using UserManagement.Interfaces;
using UserManagement.Models.Entities;
using UserManagement.Services;

namespace UserManagement.Tests.Helpers;

public abstract class TestBase
{
    protected readonly Mock<UserManager<ApplicationUser>> MockUserManager;
    protected readonly Mock<IEmailService> MockEmailService;
    protected readonly Mock<IJwtTokenService> MockJwtTokenService;
    protected readonly Mock<IUserActivityService> MockActivityService;
    protected readonly Mock<IHttpContextAccessor> MockHttpContextAccessor;
    protected readonly Mock<ILogger<AuthService>> MockLogger;
    protected readonly IConfiguration Configuration;
    protected readonly UserManagementDbContext DbContext;

    protected TestBase()
    {
        // Setup mock UserManager
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        var options = new Mock<IOptions<IdentityOptions>>();
        var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
        var userValidators = new List<IUserValidator<ApplicationUser>>();
        var passwordValidators = new List<IPasswordValidator<ApplicationUser>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new Mock<IdentityErrorDescriber>();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();

        MockUserManager = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            options.Object,
            passwordHasher.Object,
            userValidators.AsEnumerable(),
            passwordValidators.AsEnumerable(),
            keyNormalizer.Object,
            errors.Object,
            services.Object,
            logger.Object);

        // Setup mock email service
        MockEmailService = new Mock<IEmailService>();

        // Setup mock JWT token service
        MockJwtTokenService = new Mock<IJwtTokenService>();

        // Setup mock activity service
        MockActivityService = new Mock<IUserActivityService>();

        // Setup mock HttpContextAccessor with a mocked HttpContext
        var mockHttpContext = new Mock<HttpContext>();
        var mockConnection = new Mock<ConnectionInfo>();
        mockConnection.Setup(c => c.RemoteIpAddress).Returns(System.Net.IPAddress.Parse("127.0.0.1"));
        mockHttpContext.Setup(c => c.Connection).Returns(mockConnection.Object);

        // Setup Headers collection with User-Agent
        var headerDictionary = new HeaderDictionary
        {
            { "User-Agent", "Test User Agent" }
        };
        var request = new Mock<HttpRequest>();
        request.Setup(r => r.Headers).Returns(headerDictionary);
        mockHttpContext.Setup(c => c.Request).Returns(request.Object);

        MockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        MockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);

        // Setup mock logger
        MockLogger = new Mock<ILogger<AuthService>>();

        // Setup configuration
        var configValues = new Dictionary<string, string?>
        {
            {"Jwt:Key", "supersecrettestkey_atleast32characters_long"},
            {"Jwt:Issuer", "test_issuer"},
            {"Jwt:Audience", "test_audience"},
            {"Jwt:TokenValidityInMinutes", "60"},
            {"Jwt:RefreshTokenValidityInDays", "7"}
        };

        Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        // Setup in-memory database
        var options2 = new DbContextOptionsBuilder<UserManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        DbContext = new UserManagementDbContext(options2);
    }

    protected void SetupHttpContextWithUser(string userId, string email)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.User).Returns(principal);

        // Setup connection info
        var mockConnection = new Mock<ConnectionInfo>();
        mockConnection.Setup(c => c.RemoteIpAddress).Returns(System.Net.IPAddress.Parse("127.0.0.1"));
        mockHttpContext.Setup(c => c.Connection).Returns(mockConnection.Object);

        // Setup Headers collection with User-Agent
        var headerDictionary = new HeaderDictionary
        {
            { "User-Agent", "Test User Agent" }
        };
        var request = new Mock<HttpRequest>();
        request.Setup(r => r.Headers).Returns(headerDictionary);
        mockHttpContext.Setup(c => c.Request).Returns(request.Object);

        MockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);
    }
}