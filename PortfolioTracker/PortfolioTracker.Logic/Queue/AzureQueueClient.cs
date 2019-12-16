using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using PortfolioTracker.Logic.Interfaces;
using PortfolioTracker.Models;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;

namespace PortfolioTracker.Logic
{
    public class AzureQueueClient : IAzureQueueClient
    {
        public static string UpdateHistoryQueue = "updatehistory";
        public static string HistoryUpdatedQueue = "historyupdated";

        private static string ConnectionString = "Endpoint=sb://portfolio-tracker-dev.servicebus.windows.net/;SharedAccessKeyName=logic-access;SharedAccessKey=J6E+1nhEJBnjrQWWZ1RLK21oH+0Z7KmzyCp5MQ3WnME=";

        private static ConcurrentDictionary<string, IQueueClient> _queueClients = new ConcurrentDictionary<string, IQueueClient>();

        private IQueueClient GetQueueClient(string queue)
        {
            if (!_queueClients.ContainsKey(queue))
                _queueClients.TryAdd(queue, new QueueClient(ConnectionString, queue));
            _queueClients.TryGetValue(queue, out var queueClient);
            return queueClient;
        }

        public async Task SendMessageAsync(string queue, string symbol, int msDelay = 0)
        {
            var queueClient = GetQueueClient(queue);

            var queueItem = new HistoryQueueItem
            {
                EventName = "UpdateHistory",
                Symbol = symbol
            };
            // Create a new message to send to the queue.
            string messageBody = JsonConvert.SerializeObject(queueItem);
            var message = new Message(Encoding.UTF8.GetBytes(messageBody));
            message.Label = "queueItem";
            message.ScheduledEnqueueTimeUtc = DateTime.UtcNow.AddMilliseconds(msDelay);

            // Send the message to the queue.
            await queueClient.SendAsync(message);
        }
    }
}