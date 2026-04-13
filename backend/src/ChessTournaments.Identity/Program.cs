using ChessTournaments.Identity.Configurations;
using ChessTournaments.Identity.Extensions;
using ChessTournaments.Identity.Services;

const string appName = "IdentityProvider";
var loggerFactory = LoggerFactory.Create(x => x.AddConsole());

AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
{
    loggerFactory
        ?.CreateLogger(typeof(Program))
        .Log(
            LogLevel.Error,
            (Exception)e.ExceptionObject,
            $"Unhandled exception during {appName} startup"
        );
};

var builder = WebApplication.CreateBuilder(args);

var isProductionEnvironment = builder.Environment.IsEnvironment("Production");

builder.Host.AddSerilogConfiguration();

// Bind configuration settings
var identitySettings = new IdentitySettings();
builder.Configuration.GetSection("Identity").Bind(identitySettings);

var accountSettings = new AccountSettings();
builder.Configuration.GetSection("Account").Bind(accountSettings);

var oidcSettings = new OidcSettings();
builder.Configuration.GetSection("Oidc").Bind(oidcSettings);

var externalLoginMicrosoftSettings = new ExternalLoginMicrosoftSettings();
builder.Configuration.GetSection("ExternalLoginMicrosoft").Bind(externalLoginMicrosoftSettings);

// Add services
builder.Services.AddApplicationSettings(builder.Configuration);
builder.Services.AddCorsConfiguration();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddIdentityConfiguration(identitySettings, accountSettings);
builder.Services.AddExternalAuthentication(externalLoginMicrosoftSettings);
builder.Services.AddOpenIddictConfiguration(
    oidcSettings,
    isProductionEnvironment,
    builder.Configuration
);
builder.Services.AddApplicationServices();
builder.Services.AddRazorComponentsConfiguration();

// Register OpenIddict Worker to seed clients and scopes
builder.Services.Configure<OidcSettings>(builder.Configuration.GetSection("Oidc"));
builder.Services.AddHostedService<OpenIddictWorker>();

var app = builder.Build();

// Configure middleware pipeline
app.ConfigureMiddlewarePipeline();
app.ConfigureEndpoints();

// Only run migrations in Development environment
// In Azure, migrations should be run separately via GitHub Actions
if (app.Environment.IsDevelopment())
{
    app.ApplyMigrations();
}

await app.SeedDatabaseAsync();

app.Run();
