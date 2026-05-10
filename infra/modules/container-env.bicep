// Azure Container Apps managed environment.
// Wired to the Log Analytics workspace so ACA streams container stdout/stderr there.

targetScope = 'resourceGroup'

@description('Container Apps environment name.')
param name string

@description('Location for the environment.')
param location string = resourceGroup().location

@description('Tags applied to the environment.')
param tags object = {}

@description('Resource ID of the Log Analytics workspace to forward logs to.')
param logAnalyticsWorkspaceId string

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' existing = {
  // Parse the workspace name from the resource ID so we can call listKeys() without needing a separate name param.
  name: last(split(logAnalyticsWorkspaceId, '/'))
}

resource env 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
  }
}

output id string = env.id
output name string = env.name
output defaultDomain string = env.properties.defaultDomain
