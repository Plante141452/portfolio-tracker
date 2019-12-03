using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using NUnit.Framework;
using PortfolioTracker.Models;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PortfolioTracker.Client.Tests
{
    public class Tests
    {

        private IQueueClient _queueClient;

        [SetUp]
        public void Setup()
        {
            _queueClient = new QueueClient("Endpoint=sb://portfolio-tracker-dev.servicebus.windows.net/;SharedAccessKeyName=test-access;SharedAccessKey=H03gkTuC9YCItb9wyvpbMpLo7di5EDxWYWa//06gbo0=", "history");
        }

        [Test]
        public async Task Test()
        {
            // Send messages.
            await SendMessagesAsync("MSFT");
            await _queueClient.CloseAsync();
        }

        [Test]
        public async Task Test2()
        {
            // Send messages.
            RegisterOnMessageHandlerAndReceiveMessages();
            await _queueClient.CloseAsync();
        }
        private void RegisterOnMessageHandlerAndReceiveMessages()
        {
            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
                AutoComplete = false
            };

            // Register the function that processes messages.
            _queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);

            Thread.Sleep(10000);
        }

        // Use this handler to examine the exceptions received on the message pump.
        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            return Task.CompletedTask;
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message.
            var messageContent = Encoding.UTF8.GetString(message.Body);

            // Complete the message so that it is not received again.
            // This can be done only if the queue Client is created in ReceiveMode.PeekLock mode (which is the default).
            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
            // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
            // If queueClient has already been closed, you can choose to not call CompleteAsync() or AbandonAsync() etc.
            // to avoid unnecessary exceptions.
        }

        private async Task SendMessagesAsync(string symbol)
        {
            var queueItem = new HistoryQueueItem
            {
                EventName = "UpdateHistory",
                Symbol = symbol
            };
            // Create a new message to send to the queue.
            string messageBody = JsonConvert.SerializeObject(queueItem);
            var message = new Message(Encoding.UTF8.GetBytes(messageBody));
            message.Label = "queueItem";
            // Send the message to the queue.
            await _queueClient.SendAsync(message);
        }
    }
}