using System.Collections.Generic;
using System.Threading.Tasks;
using PortfolioTracker.Origin.Common.Models;

namespace PortfolioTracker.Origin.DataAccess.Interfaces
{
    public interface IPortfolioDataAccess
    {
        Task<Portfolio> GetPortfolio(string portfolioId);
        Task<List<Portfolio>> SavePortfolios(List<Portfolio> portfolios);
    }
}