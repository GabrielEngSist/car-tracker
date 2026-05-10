// User-Assigned Managed Identity attached to the Container App.
// Receives Key Vault Secrets User on the vault so the app can read secrets via DefaultAzureCredential.

targetScope = 'resourceGroup'

@description('Name of the UAMI.')
param name string

@description('Location for the UAMI.')
param location string = resourceGroup().location

@description('Tags applied to the UAMI.')
param tags object = {}

resource uami 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: name
  location: location
  tags: tags
}

output id string = uami.id
output principalId string = uami.properties.principalId
output clientId string = uami.properties.clientId
output name string = uami.name
