namespace PortfolioTracker.Models
{
    public class HistoryQueueItem
    {
        public string EventName { get; set; }
        public string Symbol { get; set; }
        public StockHistory Data { get; set; }
    }
}
