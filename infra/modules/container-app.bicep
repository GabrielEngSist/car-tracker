// Container App for Car.Tracker.Api.
// Phase 1 of the AVM ACR pattern: provisions the app with a public placeholder image and NO `registries`
// block. The `registries` config (which identity to use when pulling from ACR) is wired up by the
// `postprovision` hook in azure.yaml, which runs `az containerapp registry set --identity system`
// AFTER acr-pull-role.bicep (Phase 2) has granted AcrPull to the app's system MI. Splitting it this
// way avoids the chicken-and-egg of declaring `registries` in the same Bicep pass that creates the
// role assignment (the platform would try to validate the pull before the role propagates).
//
// Identity model:
//   - System-Assigned MI: used for ACR pull (granted in Phase 2, consumed via postprovision hook).
//   - User-Assigned MI: used by the app's DefaultAzureCredential to read Key Vault secrets.
//     We set AZURE_CLIENT_ID = UAMI client ID so DefaultAzureCredential picks the UAMI.

targetScope = 'resourceGroup'

@description('Container App name.')
param name string

@description('Location.')
param location string = resourceGroup().location

@description('Tags. The orchestrator MUST include { "azd-service-name": "api" } so azd can target this app.')
param tags object = {}

@description('Resource ID of the Container Apps managed environment.')
param containerAppsEnvironmentId string

@description('Full resource ID of the User-Assigned Managed Identity to attach.')
param userAssignedIdentityId string

@description('Client ID of the User-Assigned Managed Identity (exposed to the app as AZURE_CLIENT_ID).')
param userAssignedIdentityClientId string

@description('Container image. Defaults to a public placeholder so provisioning works before any image has been pushed to ACR.')
param containerImage string = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'

@description('Target port the app listens on inside the container.')
param targetPort int = 8080

@description('Minimum number of replicas (0 = scale to zero when idle).')
@minValue(0)
@maxValue(25)
param minReplicas int = 0

@description('Maximum number of replicas.')
@minValue(1)
@maxValue(25)
param maxReplicas int = 3

@description('CPU cores. Use json() wrapper to keep Bicep from coercing to int.')
param cpu string = '0.5'

@description('Memory size, e.g. "1Gi".')
param memory string = '1Gi'

@description('Key Vault URI exposed to the app as KeyVault__Url.')
param keyVaultUri string

@description('Application Insights connection string exposed to the app as APPLICATIONINSIGHTS_CONNECTION_STRING.')
param appInsightsConnectionString string

@description('Neon Postgres host.')
param carTrackerHost string

@description('Neon Postgres database name.')
param carTrackerDatabase string

@description('Neon Postgres username.')
param carTrackerUsername string

@description('Neon Postgres port (as a string for direct env-var binding).')
param carTrackerPort string = '5432'

@description('ConsultarPlaca base URL.')
param consultarPlacaUrl string = 'https://api.consultarplaca.com.br/v2'

resource app 'Microsoft.App/containerApps@2024-03-01' = {
  name: name
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned, UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentityId}': {}
    }
  }
  properties: {
    environmentId: containerAppsEnvironmentId
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: targetPort
        transport: 'auto'
        allowInsecure: false
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
      }
      // No `registries` block. The postprovision hook in azure.yaml wires the system-assigned
      // identity to the ACR via `az containerapp registry set --identity system` after the
      // AcrPull role assignment from acr-pull-role.bicep has been applied.
    }
    template: {
      containers: [
        {
          name: 'api'
          image: containerImage
          resources: {
            cpu: json(cpu)
            memory: memory
          }
          env: [
            { name: 'ASPNETCORE_URLS', value: 'http://0.0.0.0:${targetPort}' }
            { name: 'KeyVault__Url', value: keyVaultUri }
            // Tells DefaultAzureCredential to use this specific UAMI when multiple identities are attached.
            { name: 'AZURE_CLIENT_ID', value: userAssignedIdentityClientId }
            { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsightsConnectionString }
            { name: 'CarTracker__Host', value: carTrackerHost }
            { name: 'CarTracker__Database', value: carTrackerDatabase }
            { name: 'CarTracker__Username', value: carTrackerUsername }
            { name: 'CarTracker__Port', value: carTrackerPort }
            { name: 'ConsultarPlaca__Url', value: consultarPlacaUrl }
          ]
          probes: [
            {
              type: 'Liveness'
              tcpSocket: {
                port: targetPort
              }
              initialDelaySeconds: 10
              periodSeconds: 30
            }
          ]
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
      }
    }
  }
}

output id string = app.id
output name string = app.name
output fqdn string = app.properties.configuration.ingress.fqdn
output systemAssignedPrincipalId string = app.identity.principalId
