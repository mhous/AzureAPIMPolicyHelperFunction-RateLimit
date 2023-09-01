# Azure APIM Policy Helper Function (Rate Limit)
This is an example project for using a helper function within an Azure API Management (APIM) policy.

**Use case**: Customer uses API Management to expose APIs to their downstream customers.  Within APIM, APIs have been grouped into [products](https://learn.microsoft.com/en-us/azure/api-management/api-management-howto-add-products?tabs=azure-portal) and [rate limit](https://learn.microsoft.com/en-us/azure/api-management/rate-limit-by-key-policy) policies have been applied at the product scope.

**Ask**: While rate-limiting is applied at the product scope, customer has a need for finer-grained rate limit policy application at the API and/or operation scope due to upstream APIs relying on legacy platforms that are sensitive to high volume in contrast to other APIs within the product scope.  In addition, different customers will have different rate limits.

**Solution**: Create a reusable APIM [policy fragment](https://learn.microsoft.com/en-us/azure/api-management/policy-fragments) that calls a helper function in order to retrieve rate limit configuration for a subscription/downstream customer, caches the config, and applies the rate limit.

## Architecture and Workflow
![AzureAPIMPolicyHelperFunction-RateLimit-Architecture](/docs/AzureAPIMPolicyHelperFunction-RateLimit-Architecture.png)

### Workflow
1. Client calls API exposed by APIM
2. APIM policy calls the helper function to retrieve subscription/customer rate limit configuration
3. Helper function calls out to a data store to retrieve and return data to APIM
4. APIM applies the rate limit policy, calls the upstream API, and finally returns data back to the client

## Prerequsites
This demo requires:
1. [Azure Subscription](https://portal.azure.com)
2. [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli)
3. [Azure Function Core Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=linux%2Cisolated-process%2Cnode-v4%2Cpython-v2%2Chttp-trigger%2Ccontainer-apps&pivots=programming-language-csharp)

## Setup
This demo setup will deploy the infrastructure required to run the solution:
* Resource Group
* Storage Account
* Azure Function
* API Management

### Global Variables
#### Configure the global variables 
(you will most likely need to change these names for global uniqueness)
```
export RG_NAME="apimpolicyhelper"
export RG_REGION="westus"
export APIM_NAME="apimpolicyhelper"
export FUNC_NAME="apimpolicyhelper"
export STORAGE_ACCOUNT="apimpolicyhelper"
```

### Resource Group
#### Create a resource group for this project
```
az group create --name $RG_NAME --location $RG_REGION
```

### Storage Account
#### Create a storage account for the Azure Function
```
az storage account create --name $STORAGE_ACCOUNT --resource-group $RG_NAME \
  --location $RG_REGION --sku Standard_LRS
```

### Azure Function
#### Create the Azure Function instance
```
az functionapp create --name $FUNC_NAME --resource-group $RG_NAME \
  --storage-account $STORAGE_ACCOUNT \
  --consumption-plan-location $RG_REGION \
  --functions-version 4 \
  --os-type Linux --runtime dotnet
```

#### Deploy Helper Function
```
cd function
func azure functionapp publish $FUNC_NAME
```

### API Management
#### Create the API Management instance
```
az apim create --name $APIM_NAME --resource-group $RG_NAME \
  --publisher-name Contoso --publisher-email admin@contoso.com \
  --sku-name Consumption \
  --no-wait
```

#### Create Named values within APIM
* DefaultCalls: 1
* DefaultRenewalPeriod: 10
* SubscriptionConfigLookupUrl: @("https://replaceWithUriOfAzureFunction/api/SubscriptionConfigLookup?code=replaceWithFunctionKey&sub=" + context.Subscription.Key)

#### Create a Policy fragment within APIM
* Name: RateLimitWithExternalSubscriptionConfig
* XML Policy Fragment: [Paste content from here](/apim/RateLimitWithExternalSubscriptionConfig-Fragment.xml)

#### Add Policy fragment to operation
1. Within APIM in the Azure Portal, open APIs -> Echo API -> Operation POST Create resource -> Inbound processing -> Policy code editor
2. Between the `base` and `json-to-xml` policies, add the following:
    > `<include-fragment fragment-id="RateLimitWithExternalSubscriptionConfig" />`


## Testing
You can [test the policy and view tracing](https://learn.microsoft.com/en-us/azure/api-management/api-management-howto-api-inspector) within the Azure Portal.

In addition, a simple set of test requests are [provided](/test) and can be used with the [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) VSCode extension.

## References
* [Using external services from the Azure API Management service](https://learn.microsoft.com/en-us/azure/api-management/api-management-sample-send-request)
* [Calling a Helper API in an Azure APIM Inbound Policy](https://soltisweb.com/blog/detail/2021-03-16-callinahelperapiinanazureapiminboundpolicy)