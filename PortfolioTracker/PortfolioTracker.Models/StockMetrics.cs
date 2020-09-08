using System;

namespace PortfolioTracker.Models
{
    public class StockMetrics
    {
        public string Symbol { get; set; }
        public DateTimeOffset RsiUpdatedDate { get; set; }
        public DateTimeOffset MacdUpdatedDate { get; set; }
        public double OverallScore { get; set; }
        public double RsiScore { get; set; }
        public double MacdScore { get; set; }
    }
}