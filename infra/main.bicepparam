using './main.bicep'

// Change this before deploying. The name has to be free across azurewebsites.net.
param webAppName = 'number-to-words-demo'
param location = 'australiaeast'
param environmentName = 'Production'
