using System;
using PortfolioTracker.Client;

namespace PortfolioTracker.StockEventListener
{
    public class Program
    {
        private static readonly AlphaQueueWrapper AlphaQueue;

        static Program()
        {
            AlphaQueue = new AlphaQueueWrapper();
        }

        static void Main(string[] args)
        {
            AlphaQueue.StartQueueListener();
            
            AlphaQueue.AlphaQueue.QueueMessage(new QueueMessage { EventType = AlphaQueueWrapper.UpdateWeeklyEvent, Content = "MSFT" }).GetAwaiter().GetResult();
            AlphaQueue.AlphaQueue.QueueMessage(new QueueMessage { EventType = AlphaQueueWrapper.UpdateWeeklyEvent, Content = "SPY" }).GetAwaiter().GetResult();;
            AlphaQueue.AlphaQueue.QueueMessage(new QueueMessage { EventType = AlphaQueueWrapper.UpdateWeeklyEvent, Content = "QQQ" }).GetAwaiter().GetResult();;
            AlphaQueue.AlphaQueue.QueueMessage(new QueueMessage { EventType = AlphaQueueWrapper.UpdateWeeklyEvent, Content = "DIA" }).GetAwaiter().GetResult();;
            AlphaQueue.AlphaQueue.QueueMessage(new QueueMessage { EventType = AlphaQueueWrapper.UpdateWeeklyEvent, Content = "QCOM" }).GetAwaiter().GetResult();;
            AlphaQueue.AlphaQueue.QueueMessage(new QueueMessage { EventType = AlphaQueueWrapper.UpdateWeeklyEvent, Content = "SQ" }).GetAwaiter().GetResult();;
            AlphaQueue.AlphaQueue.QueueMessage(new QueueMessage { EventType = AlphaQueueWrapper.UpdateWeeklyEvent, Content = "V" }).GetAwaiter().GetResult();;

            Console.ReadKey();
        }
    }
}