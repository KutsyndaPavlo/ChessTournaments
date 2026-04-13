using ChessTournaments.API.Extensions;
using ChessTournaments.API.Models;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddAzureKeyVaultConfiguration();

builder.Host.UseSerilog(
    (context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration)
);

// Bind OIDC settings for authentication
var oidcSettings = new OidcSettings();
builder.Configuration.GetSection("Oidc").Bind(oidcSettings);

// Add services to the container
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddCorsConfiguration();
builder.Services.AddApiDocumentation();
builder.Services.AddOpenIddictValidation(oidcSettings);
builder.Services.AddHealthChecksConfiguration(builder.Configuration);
builder.Services.AddModules(builder.Configuration);
builder.Services.AddRateLimiterConfiguration();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseStaticFiles();
app.UseSerilogRequestLogging();
app.ConfigureMiddlewarePipeline();
app.ConfigureDevelopmentPipeline();
app.MapEndpoints();

app.Run();
