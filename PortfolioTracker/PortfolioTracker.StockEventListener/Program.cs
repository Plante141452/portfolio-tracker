using PortfolioTracker.Client;
using System;

namespace PortfolioTracker.StockEventListener
{
    public class Program
    {
        private IQueueWrapper _queue;

        public Program()
        {
            _queue = new QueueWrapper("stock-events");
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}
