using System.Collections.Generic;
using System.Linq;
using PortfolioTracker.Origin.Common.Models;

namespace PortfolioTracker.Origin.Common.Logic.Extensions
{
    public static class PortfolioExtensions
    {
        public static List<Portfolio> Clone(this IEnumerable<Portfolio> portfolios)
        {
            return portfolios?.Select(p => p.Clone()).ToList();
        }

        public static Portfolio Clone(this Portfolio portfolio)
        {
            return new Portfolio
            {
                CashOnHand = portfolio.CashOnHand,
                Categories = portfolio.Categories.Clone(),
                Stocks = portfolio.Stocks.Clone(),
                Name = portfolio.Name
            };
        }

        public static List<Category> Clone(this IEnumerable<Category> categories)
        {
            return categories?.Select(p => p.Clone()).ToList();
        }

        public static Category Clone(this Category category)
        {
            return new Category
            {
                Name = category.Name,
                Categories = category.Categories.Clone(),
                Stocks = category.Stocks.Clone(),
            };
        }

        public static List<StockAllocation> Clone(this IEnumerable<StockAllocation> stocks)
        {
            return stocks?.Select(p => p.Clone()).ToList();
        }

        public static StockAllocation Clone(this StockAllocation stock)
        {
            return new StockAllocation
            {
                Symbol = stock.Symbol,
                CurrentShares = stock.CurrentShares,
                DesiredAmount = stock.DesiredAmount,
                DesiredAmountType = stock.DesiredAmountType
            };
        }
    }
}