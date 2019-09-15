using System;

namespace PortfolioTracker.Origin.Common.Models
{
    public class Quote
    {
        public string Symbol { get; set; }
        public decimal Price { get; set; }
        public DateTime QuoteDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public long Volume { get; set; }
    }
}