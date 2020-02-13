using System.Collections.Generic;
using System.Threading.Tasks;
using PortfolioTracker.Models;

namespace PortfolioTracker.DataAccess.Interfaces
{
    public interface IPortfolioDataAccess
    {
        Task<Portfolio> GetPortfolio(string portfolioId);
        Task<List<Portfolio>> SavePortfolios(List<Portfolio> portfolios);
    }
}