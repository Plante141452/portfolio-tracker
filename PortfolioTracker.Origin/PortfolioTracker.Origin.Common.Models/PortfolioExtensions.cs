using System.Linq;

namespace PortfolioTracker.Origin.Common.Models
{
    public static class PortfolioExtensions
    {
        public static Portfolio Copy(this Portfolio portfolio)
        {
            return new Portfolio
            {
                Name = portfolio.Name,
                Categories = portfolio.Categories?.Select(c => Copy((Category)c)).ToList(),
                Stocks = portfolio.Stocks?.Select(s => s.Copy()).ToList(),
                CashOnHand = portfolio.CashOnHand
            };
        }

        public static Category Copy(this Category category)
        {
            return new Category
            {
                Name = category.Name,
                Categories = category.Categories?.Select(c => c.Copy()).ToList(),
                Stocks = category.Stocks?.Select(s => s.Copy()).ToList()
            };
        }

        public static StockAllocation Copy(this StockAllocation stock)
        {
            return new StockAllocation
            {
                Symbol = stock.Symbol,
                CurrentShares = stock.CurrentShares,
                DesiredAmountType = stock.DesiredAmountType,
                DesiredAmount = stock.DesiredAmount
            };
        }
    }
}