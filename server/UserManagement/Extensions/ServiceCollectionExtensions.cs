using UserManagement.Interfaces;
using UserManagement.Services;

namespace UserManagement.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserManagementServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IUserActivityService, UserActivityService>();
        services.AddScoped<IUserPreferencesService, UserPreferencesService>();
        services.AddHttpContextAccessor();

        return services;
    }
}