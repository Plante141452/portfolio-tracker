using PortfolioTracker.DataAccess.DataTypes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortfolioTracker.DataAccess.Interfaces
{
    public interface IPortfolioDataAccess
    {
        Task<Portfolio> GetPortfolio(string portfolioId);
        Task<List<Portfolio>> SavePortfolios(List<Portfolio> portfolios);
    }
}