using System.Threading.Tasks;

namespace PortfolioTracker.Origin.IEX
{
    public interface IIEXClient
    {
        Task<IEXQuote> GetQuote(string symbol);
    }
}