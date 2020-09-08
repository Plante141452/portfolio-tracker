using PortfolioTracker.Client;
using System;

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

            Console.ReadKey();
        }
    }
}