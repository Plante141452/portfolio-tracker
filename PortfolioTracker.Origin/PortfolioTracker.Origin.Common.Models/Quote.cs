using System;

namespace PortfolioTracker.Origin.AlphaClient
{
    public class Quote
    {
        public string Symbol { get; set; }
        public decimal Price { get; set; }
        public DateTime Time { get; set; }
        public long Volume { get; set; }
    }
}