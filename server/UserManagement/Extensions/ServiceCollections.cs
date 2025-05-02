using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Interfaces;
using UserManagement.Services;

namespace UserManagement.Extensions
{
    public static class ServiceCollections
    {
        public static IServiceCollection AddUserManagementServices(this IServiceCollection services)
        {
            // Add your custom services here
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<JwtTokenService>();

            return services;
        }
    }
}