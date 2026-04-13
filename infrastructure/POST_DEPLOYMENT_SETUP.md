# Post-Deployment Setup Guide

**IMPORTANT**: This guide is only needed if you're using a service principal with **Contributor** role instead of the recommended **Owner** role.

If you followed the recommended setup and created a service principal with **Owner** role, the role assignments are **automatically configured** during Bicep deployment and you can skip this guide.

## When to Use This Guide

You need this guide only if:
- You're using a service principal with **Contributor** role (not Owner)
- You don't have **User Access Administrator** or **Owner** role at the subscription level
- The automatic role assignments in the Bicep template failed during deployment

## Overview

When using a service principal with only **Contributor** permissions, the Bicep deployment cannot automatically assign RBAC roles to the App Service managed identity. These role assignments must be configured manually after deployment.

## Required Role Assignments

After deploying the infrastructure, you need to assign the following roles to the API App Service's Managed Identity:

### 1. Key Vault Secrets User Role

This allows the API App Service to read secrets from Key Vault.

**Using Azure CLI:**
```bash
# Get the API App Service Principal ID (from deployment outputs)
API_PRINCIPAL_ID=$(az deployment group show \
  --resource-group chess-tournaments-qa-rg \
  --name <deployment-name> \
  --query properties.outputs.apiAppServicePrincipalId.value \
  --output tsv)

# Get Key Vault Name (from deployment outputs)
KEY_VAULT_NAME=$(az deployment group show \
  --resource-group chess-tournaments-qa-rg \
  --name <deployment-name> \
  --query properties.outputs.keyVaultName.value \
  --output tsv)

# Assign Key Vault Secrets User role
az role assignment create \
  --role "Key Vault Secrets User" \
  --assignee-object-id $API_PRINCIPAL_ID \
  --assignee-principal-type ServicePrincipal \
  --scope "/subscriptions/$(az account show --query id -o tsv)/resourceGroups/chess-tournaments-qa-rg/providers/Microsoft.KeyVault/vaults/$KEY_VAULT_NAME"
```

**Using Azure Portal:**
1. Navigate to your Key Vault resource
2. Go to **Access control (IAM)** → **+ Add** → **Add role assignment**
3. Select role: **Key Vault Secrets User**
4. Click **Next**
5. Select **Managed identity**
6. Click **+ Select members**
7. Search for your API App Service name (e.g., `chess-tournaments-api-qa`)
8. Click **Select** → **Review + assign**

### 2. Storage Blob Data Contributor Role

This allows the API App Service to read/write blobs in the Storage Account.

**Using Azure CLI:**
```bash
# Get Storage Account Name (from deployment outputs)
STORAGE_ACCOUNT_NAME=$(az deployment group show \
  --resource-group chess-tournaments-qa-rg \
  --name <deployment-name> \
  --query properties.outputs.storageAccountName.value \
  --output tsv)

# Assign Storage Blob Data Contributor role
az role assignment create \
  --role "Storage Blob Data Contributor" \
  --assignee-object-id $API_PRINCIPAL_ID \
  --assignee-principal-type ServicePrincipal \
  --scope "/subscriptions/$(az account show --query id -o tsv)/resourceGroups/chess-tournaments-qa-rg/providers/Microsoft.Storage/storageAccounts/$STORAGE_ACCOUNT_NAME"
```

**Using Azure Portal:**
1. Navigate to your Storage Account resource
2. Go to **Access control (IAM)** → **+ Add** → **Add role assignment**
3. Select role: **Storage Blob Data Contributor**
4. Click **Next**
5. Select **Managed identity**
6. Click **+ Select members**
7. Search for your API App Service name (e.g., `chess-tournaments-api-qa`)
8. Click **Select** → **Review + assign**

## Complete Setup Script

Here's a complete script that assigns both roles:

```bash
#!/bin/bash

# Configuration
RESOURCE_GROUP="chess-tournaments-qa-rg"
DEPLOYMENT_NAME="chess-tournaments-deployment"  # Replace with your deployment name

echo "Fetching deployment outputs..."

# Get outputs from deployment
API_PRINCIPAL_ID=$(az deployment group show \
  --resource-group $RESOURCE_GROUP \
  --name $DEPLOYMENT_NAME \
  --query properties.outputs.apiAppServicePrincipalId.value \
  --output tsv)

KEY_VAULT_NAME=$(az deployment group show \
  --resource-group $RESOURCE_GROUP \
  --name $DEPLOYMENT_NAME \
  --query properties.outputs.keyVaultName.value \
  --output tsv)

STORAGE_ACCOUNT_NAME=$(az deployment group show \
  --resource-group $RESOURCE_GROUP \
  --name $DEPLOYMENT_NAME \
  --query properties.outputs.storageAccountName.value \
  --output tsv)

SUBSCRIPTION_ID=$(az account show --query id -o tsv)

echo "API Service Principal ID: $API_PRINCIPAL_ID"
echo "Key Vault Name: $KEY_VAULT_NAME"
echo "Storage Account Name: $STORAGE_ACCOUNT_NAME"
echo "Subscription ID: $SUBSCRIPTION_ID"

# Assign Key Vault Secrets User role
echo ""
echo "Assigning Key Vault Secrets User role..."
az role assignment create \
  --role "Key Vault Secrets User" \
  --assignee-object-id $API_PRINCIPAL_ID \
  --assignee-principal-type ServicePrincipal \
  --scope "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.KeyVault/vaults/$KEY_VAULT_NAME"

if [ $? -eq 0 ]; then
  echo "✓ Key Vault role assignment successful"
else
  echo "✗ Key Vault role assignment failed"
fi

# Assign Storage Blob Data Contributor role
echo ""
echo "Assigning Storage Blob Data Contributor role..."
az role assignment create \
  --role "Storage Blob Data Contributor" \
  --assignee-object-id $API_PRINCIPAL_ID \
  --assignee-principal-type ServicePrincipal \
  --scope "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.Storage/storageAccounts/$STORAGE_ACCOUNT_NAME"

if [ $? -eq 0 ]; then
  echo "✓ Storage Account role assignment successful"
else
  echo "✗ Storage Account role assignment failed"
fi

echo ""
echo "Post-deployment setup complete!"
echo ""
echo "Note: Role assignments may take a few minutes to propagate."
```

Save this as `setup-roles.sh` in the `infrastructure/` folder and run:
```bash
chmod +x infrastructure/setup-roles.sh
./infrastructure/setup-roles.sh
```

## Verification

After assigning roles, verify they were applied correctly:

### Verify Key Vault Access
```bash
# List role assignments for Key Vault
az role assignment list \
  --scope "/subscriptions/$(az account show --query id -o tsv)/resourceGroups/chess-tournaments-qa-rg/providers/Microsoft.KeyVault/vaults/$KEY_VAULT_NAME" \
  --query "[?principalId=='$API_PRINCIPAL_ID'].{Role:roleDefinitionName, Principal:principalId}" \
  --output table
```

### Verify Storage Account Access
```bash
# List role assignments for Storage Account
az role assignment list \
  --scope "/subscriptions/$(az account show --query id -o tsv)/resourceGroups/chess-tournaments-qa-rg/providers/Microsoft.Storage/storageAccounts/$STORAGE_ACCOUNT_NAME" \
  --query "[?principalId=='$API_PRINCIPAL_ID'].{Role:roleDefinitionName, Principal:principalId}" \
  --output table
```

## Recommended: Use Owner Service Principal Instead

Instead of following this manual setup, we recommend creating a new service principal with **Owner** role:

```bash
# Create service principal with Owner role
az ad sp create-for-rbac \
  --name "github-actions-chess-tournaments" \
  --role Owner \
  --scopes /subscriptions/{subscription-id} \
  --sdk-auth
```

**Why Owner role is recommended:**
- Role assignments are automatically configured during Bicep deployment
- No manual post-deployment steps required
- More reliable and less error-prone
- Aligns with infrastructure-as-code best practices

The role assignments are already active in [main.bicep](bicep/main.bicep) (lines 316-336) and will work automatically when using an Owner service principal.

## Troubleshooting

### Permission Denied Errors

If you see "Authorization failed" errors when assigning roles:
- Ensure you have **User Access Administrator** or **Owner** role
- Contact your Azure administrator to grant necessary permissions
- Alternatively, ask the administrator to assign the roles for you

### Role Assignment Already Exists

If you get "role assignment already exists" error:
```bash
# List existing role assignments
az role assignment list \
  --assignee $API_PRINCIPAL_ID \
  --output table
```

### API Can't Access Key Vault Secrets

If your API shows errors accessing Key Vault:
1. Verify the role assignment is in place (see Verification section)
2. Wait 5-10 minutes for role assignments to propagate
3. Restart the App Service:
```bash
az webapp restart \
  --name chess-tournaments-api-qa \
  --resource-group chess-tournaments-qa-rg
```

## Required Permissions Summary

To perform these role assignments manually, you need:
- **Contributor** role on the resource group (to view resources)
- **User Access Administrator** or **Owner** role (to assign roles)

If you don't have these permissions, ask your Azure administrator to either:
1. Grant you the necessary role, OR
2. Perform the role assignments for you using the commands above

## References

- [Azure Built-in Roles](https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles)
- [Key Vault Secrets User Role](https://learn.microsoft.com/en-us/azure/key-vault/general/rbac-guide)
- [Storage Blob Data Contributor Role](https://learn.microsoft.com/en-us/azure/storage/blobs/assign-azure-role-data-access)
- [Azure RBAC Documentation](https://learn.microsoft.com/en-us/azure/role-based-access-control/)

---

*Last Updated: January 10, 2026*
