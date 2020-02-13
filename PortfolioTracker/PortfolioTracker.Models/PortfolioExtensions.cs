using System.Collections.Generic;
using System.Linq;

namespace PortfolioTracker.Models
{
    public static class PortfolioExtensions
    {
        public static List<Portfolio> Copy(this IEnumerable<Portfolio> portfolios)
        {
            return portfolios?.Select(Copy).ToList();
        }

        public static Portfolio Copy(this Portfolio portfolio)
        {
            return new Portfolio
            {
                Id = portfolio.Id,
                Name = portfolio.Name,
                Categories = portfolio.Categories.Copy(),
                Stocks = portfolio.Stocks.Copy(),
                CashOnHand = portfolio.CashOnHand
            };
        }

        public static List<Category> Copy(this IEnumerable<Category> categories)
        {
            return categories?.Select(Copy).ToList();
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

        public static List<StockAllocation> Copy(this IEnumerable<StockAllocation> stocks)
        {
            return stocks?.Select(Copy).ToList();
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