// Azure Container Registry that hosts the app's image.
// AcrPull permission on the app's system MI is granted in a separate Phase-2 module
// (see acr-pull-role.bicep) to avoid a circular dependency between ACR and the Container App.

targetScope = 'resourceGroup'

@description('Registry name (alphanumeric only, 5-50 chars). Must be globally unique.')
@minLength(5)
@maxLength(50)
param name string

@description('Location for the ACR.')
param location string = resourceGroup().location

@description('Tags applied to the ACR.')
param tags object = {}

@description('ACR SKU.')
@allowed([
  'Basic'
  'Standard'
  'Premium'
])
param sku string = 'Basic'

resource acr 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: sku
  }
  properties: {
    adminUserEnabled: false
    publicNetworkAccess: 'Enabled'
    anonymousPullEnabled: false
  }
}

output id string = acr.id
output name string = acr.name
output loginServer string = acr.properties.loginServer
