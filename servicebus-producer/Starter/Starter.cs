using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ServiceBusDemo
{
    public static class Starter
    {
        [FunctionName("Starter")]
        [return: ServiceBus("ping", Connection = "SERVICEBUS_CONSTR")]
        public static string Run(
            [TimerTrigger("0 */1 * * * *")] TimerInfo timedStarter, 
            ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var req = new Request();
            req.RequestId = System.Guid.NewGuid().ToString();
            req.RequestStart = DateTime.Now.ToString();

            return JsonConvert.SerializeObject(req, Formatting.Indented);
        }
    }

}
