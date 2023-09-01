using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace function
{
    public static class SubscriptionConfigLookup
    {
        [FunctionName("SubscriptionConfigLookup")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string subscriptionId = req.Query["sub"];

            // For demo purposes, this function uses two APIM subscription ids representing two customers with unique rate-limit configurations.
            // Ideally, these configurations would be in an external data store (e.g. Cosmos, SQL, Redis, etc.)

            if (subscriptionId == "replaceWithSubscriptionIdForCustomer1")
            {
                return new OkObjectResult(new SubscriptionConfig() { id=subscriptionId, calls=10, renewalPeriod=20 });
            }
            else if (subscriptionId == "replaceWithSubscriptionIdForCustomer2")
            {
                return new OkObjectResult(new SubscriptionConfig() { id=subscriptionId, calls=20, renewalPeriod=30 });
            }
            else
            {
                return new NotFoundResult();
            }
        }
    }

    public class SubscriptionConfig
    {
        public string id;
        public int calls;
        public int renewalPeriod;
    }
}
