using ChessTournaments.Identity.Database;
using ChessTournaments.Identity.Database.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChessTournaments.Identity.Extensions;

public static class DatabaseSeedingExtensions
{
    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created
            await context.Database.MigrateAsync();

            // Seed roles
            await SeedRolesAsync(roleManager, logger);

            // Seed admin user
            await SeedAdminUserAsync(userManager, logger);
            await SeedUserUserAsync(userManager, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        var roles = new[] { "Admin" };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (result.Succeeded)
                {
                    logger.LogInformation("Role '{RoleName}' created successfully", roleName);
                }
                else
                {
                    logger.LogError(
                        "Failed to create role '{RoleName}': {Errors}",
                        roleName,
                        string.Join(", ", result.Errors.Select(e => e.Description))
                    );
                }
            }
            else
            {
                logger.LogInformation("Role '{RoleName}' already exists", roleName);
            }
        }
    }

    private static async Task SeedAdminUserAsync(
        UserManager<ApplicationUser> userManager,
        ILogger logger
    )
    {
        const string adminEmail = "admin@chesstournaments.com";
        const string adminPassword = "Admin@123456";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                logger.LogInformation(
                    "Admin user created successfully with email: {Email}",
                    adminEmail
                );

                // Assign Admin role
                var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (roleResult.Succeeded)
                {
                    logger.LogInformation("Admin role assigned to user: {Email}", adminEmail);
                }
                else
                {
                    logger.LogError(
                        "Failed to assign Admin role to user '{Email}': {Errors}",
                        adminEmail,
                        string.Join(", ", roleResult.Errors.Select(e => e.Description))
                    );
                }
            }
            else
            {
                logger.LogError(
                    "Failed to create admin user '{Email}': {Errors}",
                    adminEmail,
                    string.Join(", ", result.Errors.Select(e => e.Description))
                );
            }
        }
        else
        {
            logger.LogInformation("Admin user already exists with email: {Email}", adminEmail);

            // Ensure admin user has Admin role
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (roleResult.Succeeded)
                {
                    logger.LogInformation(
                        "Admin role assigned to existing user: {Email}",
                        adminEmail
                    );
                }
            }
        }
    }

    private static async Task SeedUserUserAsync(
        UserManager<ApplicationUser> userManager,
        ILogger logger
    )
    {
        const string userEmail = "user@chesstournaments.com";
        const string userPassword = "User@123456";

        var user = await userManager.FindByEmailAsync(userEmail);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = userEmail,
                Email = userEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "User",
            };

            var result = await userManager.CreateAsync(user, userPassword);

            if (result.Succeeded)
            {
                logger.LogInformation("User created successfully with email: {Email}", userEmail);
            }
            else
            {
                logger.LogError(
                    "Failed to create user '{Email}': {Errors}",
                    userEmail,
                    string.Join(", ", result.Errors.Select(e => e.Description))
                );
            }
        }
    }
}
