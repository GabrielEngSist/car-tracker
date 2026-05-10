// Azure Key Vault (RBAC mode) + role assignments.
// Secret VALUES are NOT created here — seed them after provision via:
//   az keyvault secret set --vault-name <name> --name 'CarTracker--Password' --value '<...>'
//   az keyvault secret set --vault-name <name> --name 'ConsultarPlaca--Email' --value '<...>'
//   az keyvault secret set --vault-name <name> --name 'ConsultarPlaca--ApiKey' --value '<...>'

targetScope = 'resourceGroup'

@description('Globally-unique Key Vault name (3-24 chars, alphanumeric + hyphens).')
@minLength(3)
@maxLength(24)
param name string

@description('Location for the Key Vault.')
param location string = resourceGroup().location

@description('Tags applied to the Key Vault.')
param tags object = {}

@description('Object (principal) ID of the application MI that should read secrets at runtime.')
param appPrincipalId string

@description('Object ID of the deployer (user / service principal). Optional — leave empty to skip the assignment. Granted Key Vault Secrets Officer so the deployer can both seed (set) and read secrets, plus DefaultAzureCredential works locally.')
param deployerObjectId string = ''

@description('Enable purge protection (cannot be disabled once set).')
param enablePurgeProtection bool = true

// Built-in: Key Vault Secrets User — read-only on secret values.
// https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#key-vault-secrets-user
var keyVaultSecretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6'

// Built-in: Key Vault Secrets Officer — get / list / set / delete / recover / purge secrets.
// https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#key-vault-secrets-officer
var keyVaultSecretsOfficerRoleId = 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'

resource keyVault 'Microsoft.KeyVault/vaults@2024-04-01-preview' = {
  name: name
  location: location
  tags: tags
  properties: {
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    enablePurgeProtection: enablePurgeProtection ? true : null
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow'
    }
  }
}

resource appSecretsUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, appPrincipalId, keyVaultSecretsUserRoleId)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsUserRoleId)
    principalId: appPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource deployerSecretsOfficer 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(deployerObjectId)) {
  name: guid(keyVault.id, deployerObjectId, keyVaultSecretsOfficerRoleId, 'deployer')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsOfficerRoleId)
    principalId: deployerObjectId
    // No principalType: deployer can be User, Group, or ServicePrincipal — let ARM infer.
  }
}

output id string = keyVault.id
output name string = keyVault.name
output uri string = keyVault.properties.vaultUri
