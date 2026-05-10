// Phase 2 of the AVM ACR pattern: grants AcrPull to the Container App's system-assigned MI.
// Kept in a separate module so neither the ACR module nor the Container App module depends on
// the role assignment — eliminating the circular dependency.

targetScope = 'resourceGroup'

@description('Name of the Container Registry to scope the role assignment to.')
param acrName string

@description('Principal ID of the identity that needs to pull images (typically the Container App system MI).')
param principalId string

// Built-in: AcrPull
// https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#acrpull
var acrPullRoleId = '7f951dda-4ed3-4680-a7ca-43fe172d538d'

resource acr 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' existing = {
  name: acrName
}

resource acrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(acr.id, principalId, acrPullRoleId)
  scope: acr
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', acrPullRoleId)
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}
