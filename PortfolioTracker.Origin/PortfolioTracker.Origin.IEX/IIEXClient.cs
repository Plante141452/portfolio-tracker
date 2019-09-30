using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortfolioTracker.Origin.IEX
{
    public interface IIEXClient
    {
        Task<List<IEXHistoryContract>> GetHistory(string symbol, TimeCadenceEnum cadence);
        Task<IEXQuoteContract> GetQuote(string symbol);
    }
}