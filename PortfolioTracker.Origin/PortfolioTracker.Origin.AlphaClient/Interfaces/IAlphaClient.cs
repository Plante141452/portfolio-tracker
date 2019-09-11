using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortfolioTracker.Origin.AlphaClient.Interfaces
{
    public interface IAlphaClient
    {
        Task<List<Quote>> GetQuotes(List<string> symbols);
    }
}