using Azure.Identity;

namespace ChessTournaments.API.Extensions;

public static class ConfigurationExtensions
{
    public static WebApplicationBuilder AddAzureKeyVaultConfiguration(
        this WebApplicationBuilder builder
    )
    {
        // Configure Azure Key Vault for QA and Production environments
        if (builder.Environment.IsProduction() || builder.Environment.EnvironmentName == "QA")
        {
            var keyVaultUri = builder.Configuration["KeyVault:VaultUri"];
            if (!string.IsNullOrEmpty(keyVaultUri))
            {
                builder.Configuration.AddAzureKeyVault(
                    new Uri(keyVaultUri),
                    new DefaultAzureCredential()
                );
            }
        }

        return builder;
    }
}
