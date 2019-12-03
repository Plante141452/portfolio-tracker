using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using PortfolioTracker.Origin.AlphaClient;

namespace PortfolioTracker.Origin.Functions
{
    public static class Function1
    {
        [FunctionName("QueueHistoryUpdate")]
        [return: ServiceBus("QueueHistoryUpdated", Connection = "ServiceBusConnection")]
        public static void Run([ServiceBusTrigger("history", Connection = "Endpoint=sb://portfolio-tracker-dev.servicebus.windows.net/;SharedAccessKeyName=azure-functions-access;SharedAccessKey=a7+g3dhxT1UjUj9/j5yNwBN7+6e8tRh7RIZrH7kj+qQ=")]string stockSymbol, ILogger log)
        {
            try
            {
                log.LogInformation($"C# ServiceBus queue trigger QueueHistoryUpdate function for symbol {stockSymbol} processed message.");
            }
            catch(Exception ex)
            {
                log.LogError(ex, $"C# ServiceBus queue trigger QueueHistoryUpdate function for symbol {stockSymbol} encountered error.");
            }
        }
    }
}
