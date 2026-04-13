#!/bin/bash

# Post-Deployment Role Assignment Script
# This script assigns necessary Azure RBAC roles to the API App Service's Managed Identity

set -e  # Exit on error

# Configuration
RESOURCE_GROUP="${RESOURCE_GROUP:-chess-tournaments-qa-rg}"
DEPLOYMENT_NAME="${DEPLOYMENT_NAME:-chess-tournaments-deployment}"

echo "=========================================="
echo "Post-Deployment Role Assignment Setup"
echo "=========================================="
echo ""
echo "Resource Group: $RESOURCE_GROUP"
echo "Deployment Name: $DEPLOYMENT_NAME"
echo ""

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
    echo "Error: Azure CLI is not installed."
    echo "Install it from: https://learn.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
fi

# Check if logged in
if ! az account show &> /dev/null; then
    echo "Error: Not logged in to Azure CLI."
    echo "Run: az login"
    exit 1
fi

echo "Fetching deployment outputs..."

# Get outputs from deployment
API_PRINCIPAL_ID=$(az deployment group show \
  --resource-group "$RESOURCE_GROUP" \
  --name "$DEPLOYMENT_NAME" \
  --query properties.outputs.apiAppServicePrincipalId.value \
  --output tsv 2>/dev/null)

if [ -z "$API_PRINCIPAL_ID" ]; then
    echo "Error: Could not retrieve API Service Principal ID from deployment."
    echo "Please verify:"
    echo "  1. Resource group name is correct: $RESOURCE_GROUP"
    echo "  2. Deployment name is correct: $DEPLOYMENT_NAME"
    echo "  3. Deployment has completed successfully"
    exit 1
fi

KEY_VAULT_NAME=$(az deployment group show \
  --resource-group "$RESOURCE_GROUP" \
  --name "$DEPLOYMENT_NAME" \
  --query properties.outputs.keyVaultName.value \
  --output tsv)

STORAGE_ACCOUNT_NAME=$(az deployment group show \
  --resource-group "$RESOURCE_GROUP" \
  --name "$DEPLOYMENT_NAME" \
  --query properties.outputs.storageAccountName.value \
  --output tsv)

SUBSCRIPTION_ID=$(az account show --query id -o tsv)

echo ""
echo "Retrieved configuration:"
echo "  API Service Principal ID: $API_PRINCIPAL_ID"
echo "  Key Vault Name: $KEY_VAULT_NAME"
echo "  Storage Account Name: $STORAGE_ACCOUNT_NAME"
echo "  Subscription ID: $SUBSCRIPTION_ID"
echo ""

# Assign Key Vault Secrets User role
echo "=========================================="
echo "Assigning Key Vault Secrets User role..."
echo "=========================================="

KV_SCOPE="/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.KeyVault/vaults/$KEY_VAULT_NAME"

if az role assignment create \
  --role "Key Vault Secrets User" \
  --assignee-object-id "$API_PRINCIPAL_ID" \
  --assignee-principal-type ServicePrincipal \
  --scope "$KV_SCOPE" 2>/dev/null; then
  echo "✓ Key Vault role assignment successful"
else
  # Check if role already exists
  EXISTING_ROLE=$(az role assignment list \
    --assignee "$API_PRINCIPAL_ID" \
    --scope "$KV_SCOPE" \
    --query "[?roleDefinitionName=='Key Vault Secrets User'].roleDefinitionName" \
    --output tsv)

  if [ -n "$EXISTING_ROLE" ]; then
    echo "✓ Key Vault role already assigned (skipped)"
  else
    echo "✗ Key Vault role assignment failed"
    echo "  You may not have sufficient permissions."
    echo "  Required role: User Access Administrator or Owner"
    exit 1
  fi
fi

# Assign Storage Blob Data Contributor role
echo ""
echo "=========================================="
echo "Assigning Storage Blob Data Contributor role..."
echo "=========================================="

STORAGE_SCOPE="/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.Storage/storageAccounts/$STORAGE_ACCOUNT_NAME"

if az role assignment create \
  --role "Storage Blob Data Contributor" \
  --assignee-object-id "$API_PRINCIPAL_ID" \
  --assignee-principal-type ServicePrincipal \
  --scope "$STORAGE_SCOPE" 2>/dev/null; then
  echo "✓ Storage Account role assignment successful"
else
  # Check if role already exists
  EXISTING_ROLE=$(az role assignment list \
    --assignee "$API_PRINCIPAL_ID" \
    --scope "$STORAGE_SCOPE" \
    --query "[?roleDefinitionName=='Storage Blob Data Contributor'].roleDefinitionName" \
    --output tsv)

  if [ -n "$EXISTING_ROLE" ]; then
    echo "✓ Storage Account role already assigned (skipped)"
  else
    echo "✗ Storage Account role assignment failed"
    echo "  You may not have sufficient permissions."
    echo "  Required role: User Access Administrator or Owner"
    exit 1
  fi
fi

# Verify role assignments
echo ""
echo "=========================================="
echo "Verifying role assignments..."
echo "=========================================="
echo ""

echo "Key Vault roles:"
az role assignment list \
  --scope "$KV_SCOPE" \
  --query "[?principalId=='$API_PRINCIPAL_ID'].{Role:roleDefinitionName, Principal:principalId}" \
  --output table

echo ""
echo "Storage Account roles:"
az role assignment list \
  --scope "$STORAGE_SCOPE" \
  --query "[?principalId=='$API_PRINCIPAL_ID'].{Role:roleDefinitionName, Principal:principalId}" \
  --output table

echo ""
echo "=========================================="
echo "Post-deployment setup complete!"
echo "=========================================="
echo ""
echo "Note: Role assignments may take 5-10 minutes to propagate."
echo "If your API shows access errors, wait a few minutes and restart the App Service:"
echo "  az webapp restart --name <app-name> --resource-group $RESOURCE_GROUP"
echo ""
