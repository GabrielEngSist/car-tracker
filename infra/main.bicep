// Resource-group-scoped deployment for Car.Tracker.Api Key Vault + UAMI.
// Deploy with:
//   az deployment group create -g <rg> -f infra/main.bicep \
//     -p @infra/main.parameters.json \
//     -p deployerObjectId=$(az ad signed-in-user show --query id -o tsv) \
//     -p carTrackerDbPassword='...' \
//     -p consultarPlacaEmail='...' \
//     -p consultarPlacaApiKey='...'

targetScope = 'resourceGroup'

@description('Azure region for all resources. Defaults to the resource group location.')
param location string = resourceGroup().location

@description('Short environment tag, e.g. dev / stg / prod. Used in resource names.')
@allowed([
  'dev'
  'stg'
  'prod'
])
param env string = 'dev'

@description('Globally-unique Key Vault name (3-24 lowercase alphanumeric/hyphen).')
@minLength(3)
@maxLength(24)
param keyVaultName string

@description('User-Assigned Managed Identity name attached to the future app host.')
param uamiName string = 'id-cartracker-${env}'

@description('Object (principal) ID of the user/group running deployment. Granted Key Vault Secrets User so DefaultAzureCredential works locally after az login.')
param deployerObjectId string

@secure()
@description('Neon Postgres password for the CarTracker database.')
param carTrackerDbPassword string

@secure()
@description('Email used as the username in ConsultarPlaca Basic auth.')
param consultarPlacaEmail string

@secure()
@description('API key used as the password in ConsultarPlaca Basic auth.')
param consultarPlacaApiKey string

// Built-in role: Key Vault Secrets User
// https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#key-vault-secrets-user
var keyVaultSecretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6'

resource uami 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: uamiName
  location: location
  tags: {
    app: 'car-tracker'
    env: env
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2024-04-01-preview' = {
  name: keyVaultName
  location: location
  tags: {
    app: 'car-tracker'
    env: env
  }
  properties: {
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    enablePurgeProtection: true
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow'
    }
  }
}

resource uamiSecretsUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, uami.id, keyVaultSecretsUserRoleId)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsUserRoleId)
    principalId: uami.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource deployerSecretsUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, deployerObjectId, keyVaultSecretsUserRoleId)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsUserRoleId)
    principalId: deployerObjectId
    // No principalType: the deployer may be a User or Group; letting ARM infer avoids errors when running from a non-User principal.
  }
}

// Secret names use '--' so the .NET KeyVault config provider maps them to '<section>:<key>'.
resource secretCarTrackerPassword 'Microsoft.KeyVault/vaults/secrets@2024-04-01-preview' = {
  parent: keyVault
  name: 'CarTracker--Password'
  properties: {
    value: carTrackerDbPassword
    contentType: 'text/plain'
  }
}

resource secretConsultarPlacaEmail 'Microsoft.KeyVault/vaults/secrets@2024-04-01-preview' = {
  parent: keyVault
  name: 'ConsultarPlaca--Email'
  properties: {
    value: consultarPlacaEmail
    contentType: 'text/plain'
  }
}

resource secretConsultarPlacaApiKey 'Microsoft.KeyVault/vaults/secrets@2024-04-01-preview' = {
  parent: keyVault
  name: 'ConsultarPlaca--ApiKey'
  properties: {
    value: consultarPlacaApiKey
    contentType: 'text/plain'
  }
}

@description('Vault URI to use as KeyVault:Url in the app config.')
output keyVaultUri string = keyVault.properties.vaultUri

@description('Client ID of the UAMI; attach this identity to the app host (App Service / Container Apps).')
output uamiClientId string = uami.properties.clientId

@description('Resource ID of the UAMI; required when attaching the identity to a host.')
output uamiResourceId string = uami.id
