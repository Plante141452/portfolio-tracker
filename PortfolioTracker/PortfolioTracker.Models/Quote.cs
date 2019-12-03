using System;

namespace PortfolioTracker.Models
{
    public class Quote
    {
        public string Symbol { get; set; }
        public decimal Price { get; set; }
        public DateTimeOffset QuoteDate { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
    }
}
