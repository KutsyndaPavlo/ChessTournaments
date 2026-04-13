using ChessTournaments.Identity.Database;
using Microsoft.EntityFrameworkCore;

namespace ChessTournaments.Identity.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabaseConfiguration(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(30), null);
                    sqlOptions.MigrationsHistoryTable(
                        "__EFMigrationsHistory",
                        ApplicationDbContext.IdentitySchema
                    );
                }
            );
            options.UseOpenIddict();
        });

        return services;
    }

    public static WebApplication ApplyMigrations(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();
        }

        return app;
    }
}
