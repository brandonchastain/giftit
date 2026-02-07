using './main.bicep'

param location = 'westus2'
param containerAppName = 'gifted-api'
param staticWebAppName = 'gifted-swa'
param customDomain = 'gifted.brandonchastain.com'
param storageAccountName = 'giftstgwus2'
param environmentName = 'gifted-api-env'
param logAnalyticsName = 'gifted-api-logs'
// Update this with your GitHub username or organization
// Format: ghcr.io/YOUR_GITHUB_USERNAME/gifted-api:latest
param containerImage = 'ghcr.io/brandonchastain/gifted-api:latest'
param cpuCore = '0.25'
param memorySize = '0.5Gi'
param minReplicas = 0  // Scale to zero when not in use
param maxReplicas = 1

// These should be passed at deployment time via command line:
// --parameters ghcrUsername=$env:GITHUB_USERNAME --parameters ghcrPassword=$env:CR_PAT
param ghcrUsername = ''
param ghcrPassword = ''
