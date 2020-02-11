using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace PortfolioTracker.Client
{
    public interface IQueueWrapper
    {
        Task QueueMessage(QueueMessage message);
        Task QueueMessage(QueueMessage message, TimeSpan delay);
        void HandleMessages(Action<QueueMessage> handler, Action<Exception> handleException);
        void StopListening();
    }

    public class QueueWrapper : IQueueWrapper
    {
        private readonly IQueueClient _queueClient;
        private static string _connectionString = "Endpoint=sb://portfolio-tracker.servicebus.windows.net/;SharedAccessKeyName=master;SharedAccessKey=99M8mGw/0+lBSA/gxGaYxnJsmWLIlYUo6HqdbHKuTYo=";

        public QueueWrapper(IQueueClient queueClient)
        {
            _queueClient = queueClient;
        }

        public QueueWrapper(string queueName)
            : this(new QueueClient(_connectionString, queueName))
        {
        }

        public async Task QueueMessage(QueueMessage message)
        {
            string messageContent = JsonConvert.SerializeObject(message);
            Message azureMessage = new Message(Encoding.UTF8.GetBytes(messageContent));
            await _queueClient.SendAsync(azureMessage);
        }

        public async Task QueueMessage(QueueMessage message, TimeSpan delay)
        {
            string messageContent = JsonConvert.SerializeObject(message);
            Message azureMessage = new Message(Encoding.UTF8.GetBytes(messageContent));

            var scheduleTime = DateTimeOffset.UtcNow.Add(delay);
            await _queueClient.ScheduleMessageAsync(azureMessage, scheduleTime);
        }

        public void HandleMessages(Action<QueueMessage> handler, Action<Exception> handleException)
        {
            _queueClient.RegisterMessageHandler((azureMessage, token) => Task.Run(() =>
            {
                string messageContent = Encoding.UTF8.GetString(azureMessage.Body);
                QueueMessage message = JsonConvert.DeserializeObject<QueueMessage>(messageContent);
                handler(message);
            }, token), args => Task.Run(() => handleException(args.Exception)));
        }

        public void StopListening()
        {
            _queueClient.CloseAsync().GetAwaiter().GetResult();
        }
    }

    public class QueueMessage
    {
        public string EventType { get; set; }
        public string Content { get; set; }
    }
}