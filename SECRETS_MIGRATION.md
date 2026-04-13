# Secrets Migration to Azure Key Vault

This document outlines the changes made to store all application secrets in Azure Key Vault for QA and Production environments.

## Overview

All sensitive configuration values are now stored securely in Azure Key Vault and accessed via managed identity. No secrets are stored in application configuration files or exposed in App Service settings.

## Changes Made

### 1. Infrastructure Changes ([main.bicep](infrastructure/bicep/main.bicep))

#### Added Parameters
- `oidcApiClientId`: OIDC API client ID (default: `chess-tournaments_api`)
- `oidcApiClientSecret`: OIDC API client secret (secure parameter)
- `oidcAuthority`: OIDC authority URL

#### Key Vault Secrets Created
The following secrets are automatically created in Key Vault during deployment:

| Secret Name | Contains |
|-------------|----------|
| `SqlConnectionString` | Complete SQL Server connection string with credentials |
| `StorageConnectionString` | Storage Account connection string with access key |
| `ServiceBusConnectionString` | Service Bus namespace connection string |
| `ApplicationInsightsConnectionString` | Application Insights instrumentation connection string |
| `OidcAuthority` | OpenID Connect authority URL |
| `OidcApiClientId` | OIDC API client identifier |
| `OidcApiClientSecret` | OIDC API client secret |

#### App Service Configuration
App settings now use Key Vault references instead of direct values:

```bicep
{
  name: 'ConnectionStrings__DefaultConnection'
  value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=SqlConnectionString)'
}
{
  name: 'Oidc__Authority'
  value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=OidcAuthority)'
}
// ... and more
```

### 2. Application Changes

#### NuGet Packages Added ([Directory.Packages.props](backend/Directory.Packages.props))
- `Azure.Extensions.AspNetCore.Configuration.Secrets` (v1.3.2)
- `Azure.Identity` (v1.13.1)

#### API Configuration ([Program.cs](backend/src/ChessTournaments.API/Program.cs))
Added Azure Key Vault configuration provider for QA and Production environments:

```csharp
if (builder.Environment.IsProduction() || builder.Environment.EnvironmentName == "QA")
{
    var keyVaultUri = builder.Configuration["KeyVault:VaultUri"];
    if (!string.IsNullOrEmpty(keyVaultUri))
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUri),
            new DefaultAzureCredential());
    }
}
```

### 3. Deployment Workflow Changes ([_deploy-infrastructure.yml](.github/workflows/_deploy-infrastructure.yml))

#### Additional Parameters
Deployment now passes OIDC configuration:
- `--parameters oidcApiClientSecret=${{ secrets.OIDC_API_CLIENT_SECRET }}`
- `--parameters oidcAuthority=${{ secrets.OIDC_AUTHORITY }}`

## Required GitHub Secrets

### Repository-Level Secrets
| Secret Name | Description | Example |
|-------------|-------------|---------|
| `AZURE_CREDENTIALS` | Service principal credentials | JSON from `az ad sp create-for-rbac` |
| `SQL_ADMIN_LOGIN` | SQL Server admin username | `sqladmin` |
| `SQL_ADMIN_PASSWORD` | SQL Server admin password | `MySecureP@ssw0rd123!` |

### Environment-Level Secrets (QA and Production)
| Secret Name | Description | Example |
|-------------|-------------|---------|
| `OIDC_AUTHORITY` | Identity server URL | `https://chess-tournaments-identity-qa.azurestaticapps.net/` |
| `OIDC_API_CLIENT_SECRET` | OIDC client secret | `846B62D0-DEF9-4215-A99D-86E6B8DAB342` |
| `AZURE_BACKEND_APP_PUBLISH_PROFILE` | App Service publish profile | Retrieved from Azure |
| `AZURE_IDENTITY_APP_TOKEN` | Static Web App token | Retrieved from Azure |

## Security Benefits

### Before (Insecure)
- Connection strings stored in `appsettings.json` or environment variables
- Secrets visible in App Service configuration
- No centralized secret management
- Difficult to rotate credentials

### After (Secure)
- All secrets stored in Azure Key Vault
- App Service uses Managed Identity (no credentials needed)
- Key Vault references in app settings (not actual secrets)
- Centralized secret management
- Easy credential rotation
- Audit trail for secret access
- Soft delete protection (7-day retention)

## How It Works

### Secret Flow
1. **Deployment**: GitHub Actions passes secrets to Bicep deployment
2. **Bicep**: Creates Key Vault and stores secrets
3. **App Service**: Configured with Managed Identity and Key Vault references
4. **Application**: Uses `DefaultAzureCredential` to access Key Vault
5. **Runtime**: Secrets loaded from Key Vault into configuration

### Managed Identity RBAC
The API App Service Managed Identity has:
- **Key Vault Secrets User** role on Key Vault (read secrets)
- **Storage Blob Data Contributor** role on Storage Account (upload/download blobs)

## Migration Checklist

- [x] Update Bicep template to create Key Vault secrets
- [x] Update App Service configuration to use Key Vault references
- [x] Add Azure Key Vault NuGet packages
- [x] Update API to load secrets from Key Vault
- [x] Update deployment workflows
- [x] Update documentation

## Next Steps

### For QA Environment
1. Create GitHub environment "qa" if not exists
2. Add environment secrets:
   - `OIDC_AUTHORITY`
   - `OIDC_API_CLIENT_SECRET`
   - `AZURE_BACKEND_APP_PUBLISH_PROFILE`
   - `AZURE_IDENTITY_APP_TOKEN`
3. Deploy infrastructure via GitHub Actions
4. Verify secrets in Key Vault via Azure Portal

### For Production Environment
1. Create GitHub environment "production" if not exists
2. Add environment secrets (with production values):
   - `OIDC_AUTHORITY`
   - `OIDC_API_CLIENT_SECRET`
   - `AZURE_BACKEND_APP_PUBLISH_PROFILE`
   - `AZURE_IDENTITY_APP_TOKEN`
3. Deploy infrastructure via GitHub Actions
4. Verify secrets in Key Vault via Azure Portal

## Verification

### Check Key Vault Secrets
```bash
# List all secrets in Key Vault
az keyvault secret list --vault-name <key-vault-name> --query "[].name" -o table

# View a specific secret value
az keyvault secret show --vault-name <key-vault-name> --name SqlConnectionString --query "value" -o tsv
```

### Check App Service Configuration
```bash
# List app settings (should show Key Vault references)
az webapp config appsettings list --name <app-name> --resource-group <rg-name> --query "[?contains(value, '@Microsoft.KeyVault')]"
```

### Test Application
1. Deploy the application to Azure
2. Check Application Insights for startup errors
3. Verify database connectivity (health check endpoint)
4. Verify authentication works with OIDC

## Troubleshooting

### App Can't Access Key Vault
**Symptom**: Application fails to start or can't read configuration

**Solutions**:
1. Verify Managed Identity is enabled on App Service
2. Check RBAC role assignment exists
3. Verify Key Vault URI is correct in app settings
4. Check Key Vault firewall allows Azure services

### Key Vault Reference Not Working
**Symptom**: App setting shows literal `@Microsoft.KeyVault(...)` instead of secret value

**Solutions**:
1. Ensure App Service has Managed Identity enabled
2. Verify correct Key Vault reference format
3. Check secret exists in Key Vault
4. Restart App Service to reload settings

### OIDC Authentication Fails
**Symptom**: 401 Unauthorized errors

**Solutions**:
1. Verify `OIDC_AUTHORITY` secret is correct
2. Check `OIDC_API_CLIENT_SECRET` matches Identity server configuration
3. Verify Key Vault secrets are accessible
4. Check Application Insights for detailed error messages

## References

- [Azure Key Vault Documentation](https://learn.microsoft.com/azure/key-vault/)
- [App Service Key Vault References](https://learn.microsoft.com/azure/app-service/app-service-key-vault-references)
- [Managed Identity Overview](https://learn.microsoft.com/azure/active-directory/managed-identities-azure-resources/overview)
- [DefaultAzureCredential](https://learn.microsoft.com/dotnet/api/azure.identity.defaultazurecredential)
