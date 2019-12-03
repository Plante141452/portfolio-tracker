using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PortfolioTracker.Client;
using PortfolioTracker.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PortfolioTracker.Origin.Functions
{
    public static class HistoryQueueHandle
    {
        [FunctionName("QueueHistoryUpdate")]
        [return: ServiceBus("QueueHistoryUpdate", Connection = "ServiceBusConnection")]
        public async static Task<string> Run([ServiceBusTrigger("history", Connection = "AzureWebJobsServiceBus")]string queueItem, ILogger log)
        {
            try
            {
                log.LogInformation($"C# ServiceBus queue trigger function received message: {queueItem}");
                HistoryQueueItem item = JsonConvert.DeserializeObject<HistoryQueueItem>(queueItem);

                if (item.EventName != "UpdateHistory")
                    return queueItem;
                else
                {
                    var client = new StockClient();
                    var returnObject = await client.GetHistory(item.Symbol);

                    if (returnObject.Success)
                    {
                        item.EventName = "HistoryUpdated";
                        item.Data = returnObject.Data;
                        log.LogInformation($"C# ServiceBus queue trigger function processed message: {queueItem}");
                        return JsonConvert.SerializeObject(item);
                    }

                    string message = returnObject.Messages != null
                        ? string.Join('\n', returnObject.Messages.Select(m => m.Message))
                        : "Failed to update history, requeuing";

                    log.LogError(new Exception(message), $"C# ServiceBus queue trigger function encountered error: {queueItem}");
                    return queueItem;
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"C# ServiceBus queue trigger function encountered error: {queueItem}");
                return queueItem;
            }
        }
    }
}
