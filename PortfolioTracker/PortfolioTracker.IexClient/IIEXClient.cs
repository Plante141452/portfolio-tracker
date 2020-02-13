using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortfolioTracker.IexClient
{
    public interface IIexClient
    {
        Task<List<IexHistoryContract>> GetHistory(string symbol, TimeCadenceEnum cadence);
        Task<IexQuoteContract> GetQuote(string symbol);
    }
}