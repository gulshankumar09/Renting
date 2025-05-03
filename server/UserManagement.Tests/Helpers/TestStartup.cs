using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using Microsoft.AspNetCore.Identity;
using UserManagement.Models.Entities;
using UserManagement.Interfaces;
using UserManagement.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace UserManagement.Tests.Helpers;

public class TestStartup
{
    public TestStartup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // Add DbContext with in-memory database
        services.AddDbContext<UserManagementDbContext>(options =>
            options.UseInMemoryDatabase("TestDatabase"));

        // Add Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Testing configuration - relaxed for easier testing
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<UserManagementDbContext>()
        .AddDefaultTokenProviders();

        // Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;

            var jwtKey = Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
            var issuer = Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured");
            var audience = Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured");

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });

        // Register services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IUserActivityService, UserActivityService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IUserPreferencesService, UserPreferencesService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IRoleService, RoleService>();

        services.AddControllers();
        services.AddHttpContextAccessor();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        // Seed the database
        using var scope = app.ApplicationServices.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        SeedTestUsers(userManager).Wait();
    }

    private async Task SeedTestUsers(UserManager<ApplicationUser> userManager)
    {
        // Create test users
        var testUser = new ApplicationUser
        {
            Id = "test-user-id",
            UserName = "testuser@example.com",
            Email = "testuser@example.com",
            EmailConfirmed = true,
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "1234567890",
            PhoneNumberConfirmed = true,
            RefreshToken = null
        };

        var unconfirmedUser = new ApplicationUser
        {
            Id = "unconfirmed-user-id",
            UserName = "unconfirmed@example.com",
            Email = "unconfirmed@example.com",
            EmailConfirmed = false,
            FirstName = "Unconfirmed",
            LastName = "User"
        };

        if (await userManager.FindByEmailAsync(testUser.Email) == null)
        {
            await userManager.CreateAsync(testUser, "Test@123");
        }

        if (await userManager.FindByEmailAsync(unconfirmedUser.Email) == null)
        {
            await userManager.CreateAsync(unconfirmedUser, "Test@123");
        }
    }
}