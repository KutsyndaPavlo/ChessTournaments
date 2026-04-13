# GitHub Secrets Setup Guide

This guide walks you through setting up all required GitHub secrets for the Chess Tournaments application deployment.

## Prerequisites

- Repository admin access
- Azure CLI installed and configured
- Azure subscription with Contributor access

## Step 1: Repository-Level Secrets

These secrets are shared across all environments.

### 1.1 Create Azure Service Principal

```bash
# Replace {subscription-id} with your Azure subscription ID
az ad sp create-for-rbac \
  --name "github-actions-chess-tournaments" \
  --role Owner  \
  --scopes /subscriptions/{subscription-id} \
  --sdk-auth

# Output (copy this entire JSON):
{
  "clientId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "clientSecret": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "subscriptionId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "tenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "activeDirectoryEndpointUrl": "https://login.microsoftonline.com",
  "resourceManagerEndpointUrl": "https://management.azure.com/",
  "activeDirectoryGraphResourceId": "https://graph.windows.net/",
  "sqlManagementEndpointUrl": "https://management.core.windows.net:8443/",
  "galleryEndpointUrl": "https://gallery.azure.com/",
  "managementEndpointUrl": "https://management.core.windows.net/"
}
```

### 1.2 Add Repository Secrets

Go to **Settings** → **Secrets and variables** → **Actions** → **New repository secret**

| Secret Name | Value | Notes |
|-------------|-------|-------|
| `AZURE_CREDENTIALS` | Entire JSON from Step 1.1 | Copy complete output |
| `SQL_ADMIN_LOGIN` | `sqladmin` | Choose your own username |
| `SQL_ADMIN_PASSWORD` | `MySecureP@ssw0rd123!` | Must be 8+ chars with uppercase, lowercase, numbers, symbols |

**Password Requirements:**
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one number
- At least one special character (!@#$%^&*)

## Step 2: Environment Setup

Create environments in GitHub: **Settings** → **Environments**

### 2.1 Create QA Environment

1. Click **New environment**
2. Name: `qa`
3. Click **Configure environment**
4. (Optional) Add deployment protection rules

### 2.2 Create Production Environment

1. Click **New environment**
2. Name: `production`
3. Click **Configure environment**
4. **Recommended**: Add deployment protection rules:
   - ✅ Required reviewers (select team members)
   - ✅ Wait timer (5-10 minutes)

## Step 3: QA Environment Secrets

In the **qa** environment, add the following secrets:

### 3.1 OIDC Configuration

| Secret Name | Value | How to Generate |
|-------------|-------|-----------------|
| `OIDC_AUTHORITY` | `https://chess-tournaments-identity-qa.azurestaticapps.net/` | Use your Identity Static Web App URL (after first deployment) or placeholder |
| `OIDC_API_CLIENT_SECRET` | Generate GUID | See below |

**Generate OIDC_API_CLIENT_SECRET:**

Using PowerShell:
```powershell
[Guid]::NewGuid().ToString().ToUpper()
# Example output: 846B62D0-DEF9-4215-A99D-86E6B8DAB342
```

Using Bash/CLI:
```bash
uuidgen
# Example output: 846B62D0-DEF9-4215-A99D-86E6B8DAB342
```

Using Online Tool:
- Visit: https://www.uuidgenerator.net/
- Copy the generated UUID

### 3.2 Deployment Secrets (After First Infrastructure Deployment)

These secrets are generated after deploying infrastructure for the first time.

#### Get App Service Publish Profile

```bash
# After infrastructure deployment, get the API App Service name
API_APP_NAME="chess-tournaments-api-qa"  # From deployment output
RESOURCE_GROUP="chess-tournaments-qa-rg"

# Get publish profile
az webapp deployment list-publishing-profiles \
  --name $API_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --xml
```

Copy the entire XML output and add as secret: `AZURE_BACKEND_APP_PUBLISH_PROFILE`

#### Get Static Web App Deployment Token

```bash
# Get the Static Web App name from deployment output
STATIC_WEB_APP_NAME="chess-tournaments-identity-qa"  # From deployment output
RESOURCE_GROUP="chess-tournaments-qa-rg"

# Get deployment token
az staticwebapp secrets list \
  --name $STATIC_WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --query "properties.apiKey" -o tsv
```

Add the output as secret: `AZURE_IDENTITY_APP_TOKEN`

## Step 4: Production Environment Secrets

Repeat Step 3 for the **production** environment with production-specific values:

| Secret Name | Value | Notes |
|-------------|-------|-------|
| `OIDC_AUTHORITY` | `https://chess-tournaments-identity-production.azurestaticapps.net/` | Production Identity URL |
| `OIDC_API_CLIENT_SECRET` | **Different** GUID from QA | Generate a new unique GUID |
| `AZURE_BACKEND_APP_PUBLISH_PROFILE` | Production App Service publish profile | From production deployment |
| `AZURE_IDENTITY_APP_TOKEN` | Production Static Web App token | From production deployment |

**Important**: Use a **different** `OIDC_API_CLIENT_SECRET` for production than QA for security isolation.

## Step 5: Verification

### 5.1 Check Repository Secrets

1. Go to **Settings** → **Secrets and variables** → **Actions**
2. Verify you see:
   - ✅ `AZURE_CREDENTIALS`
   - ✅ `SQL_ADMIN_LOGIN`
   - ✅ `SQL_ADMIN_PASSWORD`

### 5.2 Check Environment Secrets

1. Go to **Settings** → **Environments** → **qa**
2. Verify you see:
   - ✅ `OIDC_AUTHORITY`
   - ✅ `OIDC_API_CLIENT_SECRET`
   - ✅ `AZURE_BACKEND_APP_PUBLISH_PROFILE` (after first deployment)
   - ✅ `AZURE_IDENTITY_APP_TOKEN` (after first deployment)

3. Repeat for **production** environment

## Step 6: Initial Deployment

### 6.1 Deploy QA Infrastructure

```bash
# Trigger infrastructure deployment
git checkout develop
git add infrastructure/
git commit -m "feat: Add Key Vault secrets management"
git push origin develop
```

Or manually trigger from GitHub Actions:
1. Go to **Actions** → **Deploy Infrastructure - QA**
2. Click **Run workflow**
3. Select branch: `develop`
4. Click **Run workflow**

### 6.2 Add Post-Deployment Secrets

After infrastructure deployment completes:
1. Follow Step 3.2 to get publish profile and deployment token
2. Add them to QA environment secrets

### 6.3 Deploy Production Infrastructure

```bash
# Create and push infrastructure tag
git tag infrastructure/production/v1.0.0
git push origin infrastructure/production/v1.0.0
```

Or manually trigger from GitHub Actions:
1. Go to **Actions** → **Deploy Infrastructure - Production**
2. Click **Run workflow**
3. Click **Run workflow**

### 6.4 Add Production Post-Deployment Secrets

After production deployment completes:
1. Follow Step 3.2 (using production resource names)
2. Add them to production environment secrets

## Quick Reference

### Complete Secrets Checklist

#### Repository Secrets (3)
- [ ] `AZURE_CREDENTIALS`
- [ ] `SQL_ADMIN_LOGIN`
- [ ] `SQL_ADMIN_PASSWORD`

#### QA Environment Secrets (4)
- [ ] `OIDC_AUTHORITY`
- [ ] `OIDC_API_CLIENT_SECRET`
- [ ] `AZURE_BACKEND_APP_PUBLISH_PROFILE` (after deployment)
- [ ] `AZURE_IDENTITY_APP_TOKEN` (after deployment)

#### Production Environment Secrets (4)
- [ ] `OIDC_AUTHORITY`
- [ ] `OIDC_API_CLIENT_SECRET`
- [ ] `AZURE_BACKEND_APP_PUBLISH_PROFILE` (after deployment)
- [ ] `AZURE_IDENTITY_APP_TOKEN` (after deployment)

**Total: 11 secrets**

## Troubleshooting

### Can't Create Service Principal
**Error**: Insufficient privileges

**Solution**: Contact your Azure subscription administrator to create the service principal or grant you the required permissions.

### OIDC_AUTHORITY Unknown Before Deployment
**Solution**:
1. Use a placeholder value initially: `https://placeholder.example.com/`
2. Deploy infrastructure to get actual Static Web App URL
3. Update secret with real URL
4. Redeploy application

### Lost OIDC_API_CLIENT_SECRET
**Solution**:
1. Generate a new GUID
2. Update the GitHub secret
3. Update the secret in Azure Key Vault:
   ```bash
   az keyvault secret set \
     --vault-name <key-vault-name> \
     --name OidcApiClientSecret \
     --value "<new-guid>"
   ```
4. Restart the App Service

### Deployment Fails with "Secret Not Found"
**Solution**: Verify all required secrets are set in the correct environment (not repository-level for environment-specific secrets).

## Security Best Practices

1. **Never commit secrets** to Git
2. **Rotate credentials** regularly (every 90 days recommended)
3. **Use different secrets** for QA and Production
4. **Limit access** to GitHub secrets (repository admins only)
5. **Enable audit logging** for GitHub and Azure
6. **Use strong passwords** for SQL admin account
7. **Review secret access** in Azure Key Vault audit logs

## Additional Resources

- [GitHub Encrypted Secrets](https://docs.github.com/en/actions/security-guides/encrypted-secrets)
- [GitHub Environments](https://docs.github.com/en/actions/deployment/targeting-different-environments/using-environments-for-deployment)
- [Azure Service Principals](https://learn.microsoft.com/en-us/azure/active-directory/develop/app-objects-and-service-principals)
- [Azure Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/)
