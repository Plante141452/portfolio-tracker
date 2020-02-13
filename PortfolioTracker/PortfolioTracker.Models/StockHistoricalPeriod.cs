namespace PortfolioTracker.Models
{
    public class StockHistoricalPeriod
    {
        public string Symbol { get; set; }
        public StockHistoryItem PeriodData { get; set; }
    }
}