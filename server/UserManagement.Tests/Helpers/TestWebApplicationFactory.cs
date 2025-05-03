using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserManagement.Data;
using UserManagement.Models.Entities;

namespace UserManagement.Tests.Helpers;

public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test configuration values
            var configValues = new Dictionary<string, string?>
            {
                {"Jwt:Key", "supersecrettestkey_atleast32characters_long"},
                {"Jwt:Issuer", "test_issuer"},
                {"Jwt:Audience", "test_audience"},
                {"Jwt:TokenValidityInMinutes", "60"},
                {"Jwt:RefreshTokenValidityInDays", "7"}
            };
            config.AddInMemoryCollection(configValues);
        });

        builder.UseStartup<TestStartup>();

        builder.ConfigureTestServices(services =>
        {
            // Replace any additional services with test doubles if needed
        });
    }
}