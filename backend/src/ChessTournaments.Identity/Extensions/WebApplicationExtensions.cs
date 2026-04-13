using Carter;
using ChessTournaments.Identity.Shared.Components;

namespace ChessTournaments.Identity.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigureMiddlewarePipeline(this WebApplication app)
    {
        app.UseCors("CorsPolicy");

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAntiforgery();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    public static WebApplication ConfigureEndpoints(this WebApplication app)
    {
        app.MapCarter();
        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

        return app;
    }
}
