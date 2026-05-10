// Issues Container Apps managed certificates for one or more hostnames.
//
// Prerequisites (must be true BEFORE this module is deployed, or cert issuance hangs/fails):
//   1. DNS asuid TXT record exists for the hostname:
//        asuid.<subdomain>.<domain>  TXT  <env.customDomainVerificationId>
//        asuid                       TXT  <env.customDomainVerificationId>   (for apex)
//   2. The target record points at the Container Apps environment:
//        CNAME  →  <containerApp>.<env.defaultDomain>     (for subdomains; CNAME validation)
//        A      →  <env.staticIp>                          (for apex; HTTP validation)
//
// Outputs an array of full certificate resource IDs in the SAME ORDER as the input `hostnames`,
// so the caller can wire them into Container App `ingress.customDomains[].certificateId`.

targetScope = 'resourceGroup'

@description('Name of the Container Apps managed environment to attach certs to.')
param environmentName string

@description('Location for the cert resources (must match the environment).')
param location string = resourceGroup().location

@description('Hostnames to issue managed certs for. Each item: { hostname, validationMethod }. `validationMethod` is CNAME for subdomains and HTTP for apex domains.')
param hostnames array

resource env 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  name: environmentName
}

resource certs 'Microsoft.App/managedEnvironments/managedCertificates@2024-03-01' = [for h in hostnames: {
  parent: env
  // Cert names must be unique within the env and stable across runs (so re-deploys don't churn).
  // Replace dots with hyphens since `.` isn't allowed in resource names.
  name: 'mc-${replace(h.hostname, '.', '-')}'
  location: location
  properties: {
    subjectName: h.hostname
    domainControlValidation: h.validationMethod
  }
}]

// Resource IDs in the same order as the input array. Caller indexes into this when building
// the Container App's ingress.customDomains[].certificateId.
output certificateIds array = [for (h, i) in hostnames: certs[i].id]
