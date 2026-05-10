// AZD-shaped orchestrator for Car.Tracker.Api.
// Provisions a fresh resource group and wires all child modules (monitoring, identity, key vault,
// ACR, Container Apps environment, the app itself, and the AcrPull role assignment).
//
// Run with:
//   azd up
// or, for IaC-only:
//   az deployment sub create -l <region> -f infra/main.bicep -p @infra/main.parameters.json

targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('AZD environment name. Used as a suffix on resource names and as the resource group name.')
param environmentName string

@minLength(1)
@description('Azure region for all resources.')
param location string

@description('Object ID of the deployer (user or service principal). Granted Key Vault Secrets User so DefaultAzureCredential works locally. Leave empty to skip the assignment.')
param deployerObjectId string = ''

@description('Neon Postgres host for the prod environment.')
param carTrackerHost string = 'ep-sweet-butterfly-amaxaspp-pooler.c-5.us-east-1.aws.neon.tech'

@description('Neon Postgres database name.')
param carTrackerDatabase string = 'car_tracker'

@description('Neon Postgres username.')
param carTrackerUsername string = 'neondb_owner'

@description('Neon Postgres port.')
param carTrackerPort string = '5432'

@description('ConsultarPlaca base URL.')
param consultarPlacaUrl string = 'https://api.consultarplaca.com.br/v2'

// ---- Custom domain (two-phase rollout) ---------------------------------------------------------
// Phase A: deploy with `enableCustomDomainBindings=false` so the env exists and we can read its
//          customDomainVerificationId + staticIp from outputs. Add DNS records at the registrar.
// Phase B: set `enableCustomDomainBindings=true` to provision managed certs and bind hostnames.
//          The canonical-host middleware in the app uses the canonical+alternates env vars to
//          301-redirect apex traffic to the canonical subdomain regardless of this flag.
@description('Canonical subdomain to bind (e.g. app.example.com). Validated via CNAME. Empty disables.')
param customDomainSubdomain string = ''

@description('Optional apex domain to ALSO bind (e.g. example.com). Validated via HTTP (A record). Empty disables. When set, the app 301s traffic from this host to customDomainSubdomain.')
param customDomainApex string = ''

@description('Whether to provision managed certs + bind hostnames in this pass. Set to false on the first deploy (so you can read verification ID/static IP from outputs and configure DNS), then flip to true.')
param enableCustomDomainBindings bool = false

// Short hash that makes resource names unique within a tenant while staying deterministic per env.
var resourceSuffix = take(uniqueString(subscription().id, environmentName, location), 6)
var tags = {
  'azd-env-name': environmentName
  app: 'car-tracker'
}

resource rg 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: 'rg-${environmentName}'
  location: location
  tags: tags
}

module monitoring './modules/monitoring.bicep' = {
  name: 'monitoring'
  scope: rg
  params: {
    name: '${environmentName}-${resourceSuffix}'
    location: location
    tags: tags
  }
}

module identity './modules/identity.bicep' = {
  name: 'identity'
  scope: rg
  params: {
    name: 'id-cartracker-${resourceSuffix}'
    location: location
    tags: tags
  }
}

module keyvault './modules/keyvault.bicep' = {
  name: 'keyvault'
  scope: rg
  params: {
    // Key Vault names: 3-24 chars, alphanumeric + hyphens, globally unique.
    name: take('kv-ct-${resourceSuffix}', 24)
    location: location
    tags: tags
    appPrincipalId: identity.outputs.principalId
    deployerObjectId: deployerObjectId
  }
}

module registry './modules/registry.bicep' = {
  name: 'registry'
  scope: rg
  params: {
    // ACR names: alphanumeric only, 5-50 chars.
    name: replace('crcartracker${resourceSuffix}', '-', '')
    location: location
    tags: tags
  }
}

module containerEnv './modules/container-env.bicep' = {
  name: 'containerEnv'
  scope: rg
  params: {
    name: 'cae-cartracker-${resourceSuffix}'
    location: location
    tags: tags
    logAnalyticsWorkspaceId: monitoring.outputs.logAnalyticsWorkspaceId
  }
}

// Hostname list. Subdomain validates via CNAME, apex via HTTP. Order is informational only —
// both hostnames are passed to the same Container App and then issued separate managed certs.
//
// Three-phase orchestration handled by Bicep + postprovision hook:
//   Phase 1 (this Bicep pass): `api` module binds hostnames to the app with bindingType=Disabled
//                              (no cert). This satisfies Azure's "hostname must be registered to
//                              an app in the env before a managed cert can issue" constraint.
//   Phase 2 (this Bicep pass): `customDomain` module deploys AFTER `api` (via dependsOn) and
//                              issues a managed cert per hostname.
//   Phase 3 (postprovision hook): `az containerapp hostname bind --certificate <cert>` upgrades
//                                 each binding to SniEnabled, attaching the cert for HTTPS.
//
// Phase 3 lives outside Bicep because Bicep can't update the same resource twice in one pass
// (binding the hostname Disabled, then re-binding it SniEnabled+certId).
var subdomainEntry = customDomainSubdomain != '' ? [
  { hostname: customDomainSubdomain, validationMethod: 'CNAME' }
] : []
var apexEntry = customDomainApex != '' ? [
  { hostname: customDomainApex, validationMethod: 'HTTP' }
] : []
var customDomainHostnames = enableCustomDomainBindings ? concat(subdomainEntry, apexEntry) : []

module api './modules/container-app.bicep' = {
  name: 'api'
  scope: rg
  params: {
    name: 'ca-cartracker-${resourceSuffix}'
    location: location
    // azd-service-name is REQUIRED for azd to identify which Bicep resource maps to which service in azure.yaml.
    tags: union(tags, { 'azd-service-name': 'api' })
    containerAppsEnvironmentId: containerEnv.outputs.id
    userAssignedIdentityId: identity.outputs.id
    userAssignedIdentityClientId: identity.outputs.clientId
    keyVaultUri: keyvault.outputs.uri
    appInsightsConnectionString: monitoring.outputs.appInsightsConnectionString
    carTrackerHost: carTrackerHost
    carTrackerDatabase: carTrackerDatabase
    carTrackerUsername: carTrackerUsername
    carTrackerPort: carTrackerPort
    consultarPlacaUrl: consultarPlacaUrl
    // Just the hostnames — the container-app module binds them with bindingType=Disabled.
    customDomainHostnames: [for h in customDomainHostnames: h.hostname]
    canonicalHost: customDomainSubdomain
    // Apex is the only "alternate" host we redirect from in this setup. Comma-separated for easy
    // env-var consumption by the app.
    redirectAlternatesCsv: customDomainApex
  }
}

// Managed certs for the custom-domain hostnames. Deploys AFTER `api` (via explicit `dependsOn`)
// because Azure requires each hostname to already be registered to a Container App in the env
// before a managed cert can be issued for it. Module is a no-op when hostnames array is empty.
module customDomain './modules/custom-domain.bicep' = {
  name: 'customDomain'
  scope: rg
  params: {
    environmentName: containerEnv.outputs.name
    location: location
    hostnames: customDomainHostnames
  }
  dependsOn: [api]
}

// Phase 2: AcrPull → Container App system MI. Depends on outputs of both registry and api;
// neither of those depends on this module — no circular dependency.
module acrPullRole './modules/acr-pull-role.bicep' = {
  name: 'acrPullRole'
  scope: rg
  params: {
    acrName: registry.outputs.name
    principalId: api.outputs.systemAssignedPrincipalId
  }
}

// ---- Outputs (UPPERCASE names become azd env vars accessible via `azd env get-values`) ----

output AZURE_LOCATION string = location
output AZURE_RESOURCE_GROUP string = rg.name
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = registry.outputs.loginServer
output AZURE_CONTAINER_REGISTRY_NAME string = registry.outputs.name
output AZURE_CONTAINER_APPS_ENVIRONMENT_ID string = containerEnv.outputs.id
output AZURE_KEY_VAULT_NAME string = keyvault.outputs.name
output AZURE_KEY_VAULT_ENDPOINT string = keyvault.outputs.uri
output AZURE_LOG_ANALYTICS_WORKSPACE_ID string = monitoring.outputs.logAnalyticsWorkspaceId
output APPLICATIONINSIGHTS_CONNECTION_STRING string = monitoring.outputs.appInsightsConnectionString
output AZURE_USER_ASSIGNED_IDENTITY_ID string = identity.outputs.id
output AZURE_USER_ASSIGNED_IDENTITY_CLIENT_ID string = identity.outputs.clientId
output API_URL string = 'https://${api.outputs.fqdn}'
output SERVICE_API_NAME string = api.outputs.name

// Custom-domain DNS inputs. These are needed for Phase A (configuring records at registro.br
// before flipping enableCustomDomainBindings=true).
output AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN string = containerEnv.outputs.defaultDomain
output AZURE_CONTAINER_APPS_ENVIRONMENT_STATIC_IP string = containerEnv.outputs.staticIp
output AZURE_CONTAINER_APPS_ENVIRONMENT_VERIFICATION_ID string = containerEnv.outputs.customDomainVerificationId
