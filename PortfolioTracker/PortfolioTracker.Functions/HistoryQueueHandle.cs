using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PortfolioTracker.Client;
using PortfolioTracker.Models;
using System;
using System.Threading.Tasks;

namespace PortfolioTracker.Origin.Functions
{
    public static class HistoryQueueHandle
    {
        [FunctionName("QueueHistoryUpdate")]
        public static async Task Run([ServiceBusTrigger("updatehistory", Connection = "AzureWebJobsServiceBus")]string queueItem, ILogger log)
        {
            try
            {
                log.LogInformation($"C# ServiceBus queue trigger function received message: {queueItem}");
                HistoryQueueItem item = JsonConvert.DeserializeObject<HistoryQueueItem>(queueItem);

                var client = new StockClient();
                var result = await client.GetHistory(item.Symbol);
                log.LogInformation($"C# ServiceBus queue trigger function completed api call for symbol {item.Symbol}. Success: {result.Success}");
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"C# ServiceBus queue trigger function encountered error: {queueItem}");
            }
        }
    }
}
