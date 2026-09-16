metadata description = 'App Service plan and web app that host the Number to Words page on the free tier.'

@description('Name of the web app. This becomes the host name, so it has to be unique across azurewebsites.net.')
@minLength(2)
@maxLength(60)
param webAppName string

@description('Region for the plan and the app.')
param location string = resourceGroup().location

@description('Name of the App Service plan.')
param appServicePlanName string = '${webAppName}-plan'

@description('Runtime stack for the Linux worker.')
param linuxFxVersion string = 'DOTNETCORE|10.0'

@description('Environment name passed to ASP.NET Core.')
@allowed(['Development', 'Staging', 'Production'])
param environmentName string = 'Production'

// F1 is the free tier: one instance, no Always On, no custom TLS certificate, and a daily
// compute quota. The settings below switch off everything that would fail or cost money
// under those limits.
resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'F1'
    tier: 'Free'
    capacity: 1
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: webAppName
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true

    // One worker is the tier limit. ARR affinity would gain nothing and would pin clients to
    // an instance that may be recycled.
    clientAffinityEnabled: false

    siteConfig: {
      linuxFxVersion: linuxFxVersion
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      healthCheckPath: '/health'

      // F1 does not offer Always On. Asking for it fails the deployment.
      alwaysOn: false

      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environmentName
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
      ]
    }
  }
}

@description('The host name the app is reachable on.')
output defaultHostName string = webApp.properties.defaultHostName

@description('The full URL of the deployed page.')
output siteUrl string = 'https://${webApp.properties.defaultHostName}'

@description('The name of the web app, for the deployment workflow.')
output webAppName string = webApp.name
