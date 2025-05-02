using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Polly;
using UserManagement.Models.Entities;

namespace UserManagement.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<UserManagementDbContext>>();
        var retryPolicy = CreateRetryPolicy(logger);

        await retryPolicy.ExecuteAsync(async () =>
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<UserManagementDbContext>();
            await EnsureDatabaseIsAvailableAsync(context, logger);

            await ApplyMigrationsAsync(context, logger);
            await SeedDatabaseAsync(services, logger);
        });
    }

    private static IAsyncPolicy CreateRetryPolicy(ILogger logger)
    {
        return Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    logger.LogError(exception, "Error occurred while initializing database. Retrying ({RetryCount}/5) after {RetryTimeSpan}...",
                        retryCount, timeSpan);
                }
            );
    }

    private static async Task EnsureDatabaseIsAvailableAsync(UserManagementDbContext context, ILogger logger)
    {
        if (!await context.Database.CanConnectAsync())
        {
            throw new Exception("Cannot connect to the database.");
        }
    }

    private static async Task ApplyMigrationsAsync(UserManagementDbContext context, ILogger logger)
    {
        logger.LogInformation("Applying database migrations...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");
    }

    private static async Task SeedDatabaseAsync(IServiceProvider services, ILogger logger)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        logger.LogInformation("Seeding database...");
        await DataSeeder.SeedRolesAsync(roleManager);
        await DataSeeder.SeedDefaultAdminAsync(userManager);
        logger.LogInformation("Database seeding completed successfully.");
    }

}