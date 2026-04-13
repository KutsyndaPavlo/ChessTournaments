using Carter;
using ChessTournaments.API.Infrastructure.OpenApi;
using ChessTournaments.API.Middleware;
using ChessTournaments.Modules.Matches.API;
using ChessTournaments.Modules.Matches.Infrastructure.Persistence;
using ChessTournaments.Modules.Players.Infrastructure.Persistence;
using ChessTournaments.Modules.TournamentRequests.Infrastructure.Persistence;
using ChessTournaments.Modules.Tournaments.Infrastructure.Persistence;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

namespace ChessTournaments.API.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        // Map OpenAPI and Scalar endpoints for development
        if (app.Environment.IsDevelopment())
        {
            app.MapCustomizedOpenApiEndpoints();
        }

        app.MapControllers();
        app.MapCarter();
        app.MapMatchesEndpoints();

        // Map health check endpoints
        app.MapHealthChecks(
                "/health",
                new HealthCheckOptions
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                }
            )
            .AllowAnonymous();

        app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = _ => false })
            .AllowAnonymous();

        return app;
    }

    public static WebApplication ConfigureDevelopmentPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // Apply migrations for all module DbContexts in development
            app.ApplyModuleMigrations();
        }

        return app;
    }

    public static WebApplication ConfigureMiddlewarePipeline(this WebApplication app)
    {
        // Global exception handling - must be first
        app.UseMiddleware<GlobalExceptionMiddleware>();

        app.UseHttpsRedirection();
        app.UseCors("CorsPolicy");
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRateLimiter();

        return app;
    }

    public static WebApplication ApplyModuleMigrations(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                // Players module
                var playersDb = scope.ServiceProvider.GetRequiredService<PlayersDbContext>();
                logger.LogInformation("Applying migrations for PlayersDbContext...");
                playersDb.Database.Migrate();
                logger.LogInformation("PlayersDbContext migrations applied successfully.");

                // Tournaments module
                var tournamentsDb =
                    scope.ServiceProvider.GetRequiredService<TournamentsDbContext>();
                logger.LogInformation("Applying migrations for TournamentsDbContext...");
                tournamentsDb.Database.Migrate();
                logger.LogInformation("TournamentsDbContext migrations applied successfully.");

                // TournamentRequests module
                var tournamentRequestsDb =
                    scope.ServiceProvider.GetRequiredService<TournamentRequestsDbContext>();
                logger.LogInformation("Applying migrations for TournamentRequestsDbContext...");
                tournamentRequestsDb.Database.Migrate();
                logger.LogInformation(
                    "TournamentRequestsDbContext migrations applied successfully."
                );

                // Matches module
                var matchesDb = scope.ServiceProvider.GetRequiredService<MatchesDbContext>();
                logger.LogInformation("Applying migrations for MatchesDbContext...");
                matchesDb.Database.Migrate();
                logger.LogInformation("MatchesDbContext migrations applied successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while applying module migrations.");
                throw;
            }
        }

        return app;
    }
}
