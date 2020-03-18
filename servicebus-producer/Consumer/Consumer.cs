using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace ServiceBusDemo
{
    public static class Consumer
    {
        [FunctionName("External-Consumer")]
         public static void Run(
            [ServiceBusTrigger("pong", Connection = "SERVICEBUS_CONSTR")]string myQueueItem, 
            [CosmosDB(
                databaseName: "Requests",
                collectionName: "Items",
                ConnectionStringSetting = "COSMOSDB_CONSTR")] ICollector<Request> requestCollection,
            ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
            Request request = JsonConvert.DeserializeObject<Request>(myQueueItem);
            request.RequestEnd = DateTime.Now.ToString();
            requestCollection.Add(request);
        }
    }
}
