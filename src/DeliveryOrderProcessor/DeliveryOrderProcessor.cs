using System;
using System.IO;
using System.Threading.Tasks;
using DeliveryOrderProcessor.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DeliveryOrderProcessor
{
    public static class DeliveryOrderProcessor
    {
        [FunctionName("DeliveryOrderProcessor")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string _endpointUri = Environment.GetEnvironmentVariable("CosmosDbUri");
            string _primaryKey = Environment.GetEnvironmentVariable("CosmosDbPrimaryKey");

            log.LogInformation("C# HTTP trigger function processed a request.");

            var cosmosClientoptions = new CosmosClientOptions()
            {
                SerializerOptions = new CosmosSerializationOptions()
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            };

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Order order = JsonConvert.DeserializeObject<Order>(requestBody);

            using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey, cosmosClientoptions))
            {
                DatabaseResponse databaseResponse = await client.CreateDatabaseIfNotExistsAsync("Delivery");
                Database targetDatabase = databaseResponse.Database;
                await Console.Out.WriteLineAsync($"Database Id:\t{targetDatabase.Id}");

                var containerResponse = await targetDatabase.CreateContainerIfNotExistsAsync("Orders", "/buyerId");
                var customContainer = containerResponse.Container;
                await customContainer.CreateItemAsync<Order>(order, new PartitionKey(order.BuyerId));

                return new OkObjectResult("Order details are saved.");
            }
        }
    }
}
