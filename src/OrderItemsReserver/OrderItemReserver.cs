using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace OrderItemsReserver
{
    public class OrderItemReserver
    {
        [FunctionName("OrderItemReserver")]
        public async Task Run([ServiceBusTrigger("orders", Connection = "SbConnection")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");

            try
            {
                var newOrder = JsonSerializer.Deserialize<Order>(myQueueItem);

                string Connection = Environment.GetEnvironmentVariable("StorageConnectionString");
                string containerName = Environment.GetEnvironmentVariable("ContainerName");
                var options = new BlobClientOptions();
                options.Retry.MaxRetries = 3;

                BlobServiceClient blobServiceClient = new BlobServiceClient(Connection, options);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync();

                BlobClient blobClient = containerClient.GetBlobClient(Guid.NewGuid().ToString());

                var response = blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(myQueueItem)));
            }
            catch(Exception ex)
            {
                var uri = "https://logicapporderssp.azurewebsites.net:443/api/orders/triggers/manual/invoke?api-version=2022-05-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=lrW8D2u0Dw_im59pgyvlsqjfpx5L7G36ckk0dBhSiUw";
                var client = new HttpClient();

                var data = new StringContent(myQueueItem, Encoding.UTF8, "application/json");
                await client.PostAsync(uri, data);

                log.LogInformation("Order details were sent to logic app");

                log.LogError(ex, ex.Message);
                log.LogError("Something went wrong.");

            }
        }
    }
}
