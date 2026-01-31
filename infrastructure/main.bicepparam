using './main.bicep'

param location = 'westus2'
param containerAppName = 'gifted-api'
param staticWebAppName = 'gifted-swa'
param customDomain = 'gifted.brandonchastain.com'
param acrName = 'giftedacr'
param storageAccountName = 'giftstgwus2'
param environmentName = 'gifted-api-env'
param logAnalyticsName = 'gifted-api-logs'
param containerImage = 'giftedacr.azurecr.io/gifted-api:latest'
param cpuCore = '0.25'
param memorySize = '0.5Gi'
param minReplicas = 0  // Scale to zero when not in use
param maxReplicas = 1
