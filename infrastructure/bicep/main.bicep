targetScope = 'resourceGroup'

@description('The environment name (qa, production)')
param environmentName string

@description('The location for all resources')
param location string

@description('The location for Static Web App (must be one of: westus2, centralus, eastus2, westeurope, eastasia)')
param staticWebAppLocation string

@description('The location for SQL Server and Database (use a region that accepts new SQL servers)')
param sqlServerLocation string

@description('The name prefix for all resources')
param projectName string = 'chess-tournaments'

@description('SQL Server administrator login')
@secure()
param sqlAdminLogin string

@description('SQL Server administrator password')
@secure()
param sqlAdminPassword string

@description('App Service Plan SKU for API')
param apiAppServicePlanSku object = {
  name: 'F1'
  tier: 'Free'
  capacity: 1
}

@description('SQL Database SKU')
param sqlDatabaseSku object = {
  name: 'Basic'
  tier: 'Basic'
  capacity: 5
}

@description('Service Bus SKU')
param serviceBusSku string = 'Standard'

@description('OIDC API Client ID')
param oidcApiClientId string = 'chess-tournaments_api'

@description('OIDC API Client Secret')
@secure()
param oidcApiClientSecret string

@description('Tags to apply to all resources')
param tags object = {
  Environment: environmentName
  Project: projectName
  ManagedBy: 'Bicep'
}

// Variables
var uniqueSuffix = uniqueString(resourceGroup().id, environmentName)
var apiAppName = '${projectName}-api-${environmentName}-${take(uniqueSuffix, 6)}'
var identityStaticWebAppName = '${projectName}-identity-${environmentName}-${take(uniqueSuffix, 6)}'
var frontendStaticWebAppName = '${projectName}-frontend-${environmentName}-${take(uniqueSuffix, 6)}'
var sqlServerName = '${projectName}-sql-${environmentName}-${uniqueSuffix}'
var sqlDatabaseName = '${projectName}-db-${environmentName}'
var keyVaultName = 'kv-${take(environmentName, 2)}-c${take(uniqueSuffix, 11)}'
var storageAccountName = 'chesstourn${take(environmentName, 2)}${take(uniqueSuffix, 8)}'
var serviceBusNamespaceName = '${projectName}-sb-${environmentName}-${uniqueSuffix}'
var appServicePlanName = '${projectName}-asp-${environmentName}'
var appInsightsName = '${projectName}-ai-${environmentName}'
var logAnalyticsWorkspaceName = '${projectName}-law-${environmentName}'

// Log Analytics Workspace
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// Application Insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

// Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' = {
  name: keyVaultName
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
  }
}

// Storage Account
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  tags: tags
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
  }
}

// Blob Container
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    deleteRetentionPolicy: {
      enabled: true
      days: 7
    }
  }
}

resource blobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobService
  name: 'uploads'
  properties: {
    publicAccess: 'None'
  }
}

// SQL Server
resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: sqlServerName
  location: sqlServerLocation
  tags: tags
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

// SQL Database
resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: sqlServerLocation
  tags: tags
  sku: sqlDatabaseSku
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: sqlDatabaseSku.tier == 'Free' ? 33554432 : 2147483648 // Free tier: 32MB, others: 2GB
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
  }
}

// SQL Firewall Rule - Allow Azure Services
resource sqlFirewallRule 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = {
  parent: sqlServer
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Service Bus Namespace
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusNamespaceName
  location: location
  tags: tags
  sku: {
    name: serviceBusSku
    tier: serviceBusSku
  }
  properties: {
    minimumTlsVersion: '1.2'
  }
}

// Service Bus Queue Example
resource serviceBusQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: 'tournament-events'
  properties: {
    maxDeliveryCount: 10
    lockDuration: 'PT5M'
    defaultMessageTimeToLive: 'P14D'
  }
}

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: appServicePlanName
  location: location
  tags: tags
  sku: apiAppServicePlanSku
  kind: 'linux'
  properties: {
    reserved: true
  }
}

// Identity App Service (Blazor Server)
// NOTE: This must be defined before apiAppService so we can reference its URL in the API's OIDC settings
resource identityAppService 'Microsoft.Web/sites@2023-01-01' = {
  name: identityStaticWebAppName
  location: location
  tags: tags
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    clientAffinityEnabled: true // Blazor Server needs sticky sessions
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: apiAppServicePlanSku.tier != 'Free'
      http20Enabled: true
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=ApplicationInsightsConnectionString)'
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environmentName == 'production' ? 'Production' : 'Development'
        }
        {
          name: 'KeyVault__VaultUri'
          value: keyVault.properties.vaultUri
        }
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=SqlConnectionString)'
        }
        {
          name: 'Oidc__Authority'
          value: 'https://${identityStaticWebAppName}.azurewebsites.net/'
        }
        {
          name: 'Oidc__Issuer'
          value: 'https://${identityStaticWebAppName}.azurewebsites.net/'
        }
      ]
      connectionStrings: []
    }
  }
}

// API App Service
resource apiAppService 'Microsoft.Web/sites@2023-01-01' = {
  name: apiAppName
  location: location
  tags: tags
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    clientAffinityEnabled: false
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: apiAppServicePlanSku.tier != 'Free'  // AlwaysOn not supported in Free tier
      http20Enabled: true
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      healthCheckPath: '/health'
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=ApplicationInsightsConnectionString)'
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environmentName == 'production' ? 'Production' : 'Development'
        }
        {
          name: 'KeyVault__VaultUri'
          value: keyVault.properties.vaultUri
        }
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=SqlConnectionString)'
        }
        {
          name: 'ConnectionStrings__StorageConnection'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=StorageConnectionString)'
        }
        {
          name: 'ConnectionStrings__ServiceBusConnection'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=ServiceBusConnectionString)'
        }
        {
          name: 'Oidc__Authority'
          value: 'https://${identityAppService.properties.defaultHostName}/'
        }
        {
          name: 'Oidc__Issuer'
          value: 'https://${identityAppService.properties.defaultHostName}/'
        }
        {
          name: 'Oidc__API__ClientId'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=OidcApiClientId)'
        }
        {
          name: 'Oidc__API__ClientSecret'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=OidcApiClientSecret)'
        }
      ]
      connectionStrings: []
    }
  }
}

// Frontend Angular Static Web App
resource frontendStaticWebApp 'Microsoft.Web/staticSites@2023-01-01' = {
  name: frontendStaticWebAppName
  location: staticWebAppLocation
  tags: tags
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {
    stagingEnvironmentPolicy: 'Enabled'
    allowConfigFileUpdates: true
    provider: 'Custom'
    enterpriseGradeCdnStatus: 'Disabled'
  }
}

// Role Assignments - API App to Key Vault
// Note: Requires Owner or User Access Administrator role on the subscription
resource apiAppKeyVaultSecretsUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, apiAppService.id, 'SecretsUser')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6') // Key Vault Secrets User
    principalId: apiAppService.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Role Assignments - Identity App to Key Vault
// Note: Requires Owner or User Access Administrator role on the subscription
resource identityAppKeyVaultSecretsUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, identityAppService.id, 'SecretsUser')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6') // Key Vault Secrets User
    principalId: identityAppService.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Role Assignments - API App to Storage Account
// Note: Requires Owner or User Access Administrator role on the subscription
resource apiAppStorageBlobDataContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, apiAppService.id, 'BlobContributor')
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe') // Storage Blob Data Contributor
    principalId: apiAppService.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Store connection strings in Key Vault
resource sqlConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  parent: keyVault
  name: 'SqlConnectionString'
  properties: {
    value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabase.name};Persist Security Info=False;User ID=${sqlAdminLogin};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
  }
}

resource storageConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  parent: keyVault
  name: 'StorageConnectionString'
  properties: {
    value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
  }
}

resource serviceBusConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  parent: keyVault
  name: 'ServiceBusConnectionString'
  properties: {
    value: listKeys('${serviceBusNamespace.id}/AuthorizationRules/RootManageSharedAccessKey', serviceBusNamespace.apiVersion).primaryConnectionString
  }
}

resource appInsightsConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  parent: keyVault
  name: 'ApplicationInsightsConnectionString'
  properties: {
    value: appInsights.properties.ConnectionString
  }
}

resource oidcApiClientIdSecret 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  parent: keyVault
  name: 'OidcApiClientId'
  properties: {
    value: oidcApiClientId
  }
}

resource oidcApiClientSecretSecret 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  parent: keyVault
  name: 'OidcApiClientSecret'
  properties: {
    value: oidcApiClientSecret
  }
}

// Outputs
output apiAppServiceName string = apiAppService.name
output apiAppServiceUrl string = 'https://${apiAppService.properties.defaultHostName}'
output identityStaticWebAppName string = identityAppService.name
output identityStaticWebAppUrl string = 'https://${identityAppService.properties.defaultHostName}'
output frontendStaticWebAppName string = frontendStaticWebApp.name
output frontendStaticWebAppUrl string = 'https://${frontendStaticWebApp.properties.defaultHostname}'
output sqlServerName string = sqlServer.name
output sqlDatabaseName string = sqlDatabase.name
output keyVaultName string = keyVault.name
output keyVaultUri string = keyVault.properties.vaultUri
output storageAccountName string = storageAccount.name
output serviceBusNamespaceName string = serviceBusNamespace.name
output appInsightsConnectionString string = appInsights.properties.ConnectionString
output apiAppServicePrincipalId string = apiAppService.identity.principalId
