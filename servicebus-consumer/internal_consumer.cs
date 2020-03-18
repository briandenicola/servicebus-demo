using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace ServiceBusDemo
{
    public static class internal_consumer
    {
        [FunctionName("Internal-Consumer")]
        [return: ServiceBus("pong", Connection = "SERVICEBUS_CONSTR")]
        public static async Task<string> Run(
            [ServiceBusTrigger("ping", Connection = "SERVICEBUS_CONSTR")] string myQueueItem, 
            ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
            Request request = JsonConvert.DeserializeObject<Request>(myQueueItem);
            request.RequestApiStamp = await GetApiTimeStamp(); 
            return JsonConvert.SerializeObject(request, Formatting.Indented);
        }

        public static async Task<string> GetApiTimeStamp()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("API_ENDPOINT"));
                HttpResponseMessage response = await client.GetAsync("api/ping");

                if (response.IsSuccessStatusCode)
                {
                    Api api = await response.Content.ReadAsAsync<Api>();
                    return api.TimeStamp;
                }
            }

            return String.Empty; 
        }
    }
}

