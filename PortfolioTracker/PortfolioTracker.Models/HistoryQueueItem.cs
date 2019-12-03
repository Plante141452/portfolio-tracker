using Newtonsoft.Json;

namespace PortfolioTracker.Models
{
    public class HistoryQueueItem
    {
        [JsonProperty]
        public string EventName { get; set; }

        [JsonProperty]
        public string Symbol { get; set; }

        [JsonProperty]
        public StockHistory Data { get; set; }
    }
}
