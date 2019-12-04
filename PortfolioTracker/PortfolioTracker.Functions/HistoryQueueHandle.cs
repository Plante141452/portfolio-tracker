using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PortfolioTracker.Client;
using PortfolioTracker.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PortfolioTracker.Origin.Functions
{
    public static class HistoryQueueHandle
    {
        [FunctionName("QueueHistoryUpdate")]
        public async static Task Run(
            [ServiceBusTrigger("updatehistory", Connection = "AzureWebJobsServiceBus")] string queueItem, 
            ILogger log,
            [ServiceBus("updatehistory", Connection = "AzureWebJobsServiceBus", EntityType = EntityType.Queue)] IAsyncCollector<string> reQueue,
            [ServiceBus("historyupdated", Connection = "AzureWebJobsServiceBus", EntityType = EntityType.Queue)] IAsyncCollector<string> outputQueue)
        {
            try
            {
                log.LogInformation($"C# ServiceBus queue trigger function received message: {queueItem}");
                HistoryQueueItem item = JsonConvert.DeserializeObject<HistoryQueueItem>(queueItem);

                if (item.EventName != "UpdateHistory")
                {
                    await reQueue.AddAsync(queueItem);
                    return;
                }

                var client = new StockClient();
                var returnObject = await client.GetHistory(item.Symbol);

                if (returnObject.Success)
                {
                    item.EventName = "HistoryUpdated";
                    item.Data = returnObject.Data;
                    log.LogInformation($"C# ServiceBus queue trigger function processed message: {queueItem}");
                    string output = JsonConvert.SerializeObject(item);
                    await outputQueue.AddAsync(output);
                    return;
                }

                string message = returnObject.Messages != null
                    ? string.Join('\n', returnObject.Messages.Select(m => m.Message))
                    : "Failed to update history, requeuing";

                log.LogError(new Exception(message), $"C# ServiceBus queue trigger function encountered error: {queueItem}");
                await reQueue.AddAsync(queueItem);
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"C# ServiceBus queue trigger function encountered error: {queueItem}");
                await reQueue.AddAsync(queueItem);
            }
        }
    }
}
