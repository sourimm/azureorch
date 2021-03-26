using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DurableFunctionOrch
{
    public static class orch
    {
        [FunctionName("orch")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            // outputs.Add(await context.CallActivityAsync<string>("orch_Hello", "Tokyo"));
            // outputs.Add(await context.CallActivityAsync<string>("orch_Hello", "Seattle"));
            // outputs.Add(await context.CallActivityAsync<string>("orch_Hello", "London"));

            SayHelloRequest data = context.GetInput<SayHelloRequest>();
            foreach (var city in data.CityNames)
            {
                outputs.Add(await context.CallActivityAsync<string>("orch_Hello", city));
            }

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [FunctionName("orch_Hello")]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName("orch_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var data = await req.Content.ReadAsAsync<SayHelloRequest>();
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("orch", data);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }

    class SayHelloRequest
    {
        public List<string> CityNames { get; set; }
    }
}