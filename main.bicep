param storageAccountType string = 'Standard_LRS'

param functionAppName string = 'func-poketraderapi'

@description('Location for all resources.')
param location string = resourceGroup().location

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: 'sapoketraderapi'
  location: location
  kind: 'StorageV2'
  sku: {
    name: storageAccountType
  }
}

resource dataStorageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: 'sapoketraderdata'
  location: location
  properties: {}
  kind: 'StorageV2'
  sku: {
    name: storageAccountType
  }
}

resource dataTableService 'Microsoft.Storage/storageAccounts/tableServices@2023-01-01' = {
  name: 'default'
  parent: dataStorageAccount
}


resource buyOrders 'Microsoft.Storage/storageAccounts/tableServices/tables@2023-01-01' = {
  name: 'buyorders'
  parent: dataTableService
}
resource pokemonSellOrders 'Microsoft.Storage/storageAccounts/tableServices/tables@2023-01-01' = {
  name: 'pokemonsellorders'
  parent: dataTableService
}

resource pokeTraderQueueService 'Microsoft.Storage/storageAccounts/queueServices@2023-01-01' = {
  name: 'default'
  parent: dataStorageAccount
}

resource placeBuyOrderQueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-01-01' = {
  name: 'place-buy-order'
  parent: pokeTraderQueueService
}


var hostingPlanName = functionAppName

resource hostingPlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: hostingPlanName
  location: location
  sku: {
    name: 'Y1'
  }
  properties: {}
}

resource functionApp 'Microsoft.Web/sites@2022-03-01' = {
  name: functionAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  kind: 'functionapp'
  properties: {
    serverFarmId: hostingPlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: toLower(functionAppName)
        }
        {
          name: 'special__queueServiceUri'
          value: dataStorageAccount.properties.primaryEndpoints.queue
        }
        {
          name: 'special__tableServiceUri'
          value: dataStorageAccount.properties.primaryEndpoints.table
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
      ]
    }
  }
}

// @allowed([ 'Free', 'Standard' ])
// param sku string = 'Free'

// resource staticWebApp 'Microsoft.Web/staticSites@2022-09-01' = {
//  location: location
//  name: 'poketrader-nuxtjsapp'
//  sku: {
//   name: sku
//   size: sku
//  }
// }

resource storageTableDataContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
}

resource storageQueueDataMessageProcessor 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '8a0f0c08-91a1-4084-bc3d-661d67233fed'
}

resource storageQueueDataMessageSender 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: 'c6a89b2d-59bc-44d0-9896-0f6e12d7b80a'
}


resource tableContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, functionApp.id, storageTableDataContributorRoleDefinition.id)
  scope: dataStorageAccount
  properties: {
    roleDefinitionId: storageTableDataContributorRoleDefinition.id
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource queueProcessorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, functionApp.id, storageQueueDataMessageProcessor.id)
  scope: dataStorageAccount
  properties: {
    roleDefinitionId: storageQueueDataMessageProcessor.id
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource queueSenderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, functionApp.id, storageQueueDataMessageSender.id)
  scope: dataStorageAccount
  properties: {
    roleDefinitionId: storageQueueDataMessageSender.id
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}
