using System.Collections.Generic;
using System.Linq;
using PortfolioTracker.Origin.Common.Models;

namespace PortfolioTracker.Origin.Common.Logic.Extensions
{
    public static class PortfolioHistoryPeriodExtensions
    {
        public static PortfolioHistoryPeriod FilterCopy(this PortfolioHistoryPeriod historyPeriod, List<string> symbols)
        {
            return new PortfolioHistoryPeriod
            {
                ClosingDate = historyPeriod.ClosingDate,
                Stocks = historyPeriod.Stocks.Where(s => symbols.Contains(s.Symbol)).Select(s => new StockHistoricalPeriod
                {
                    Symbol = s.Symbol,
                    PeriodData = new StockHistoryItem
                    {
                        AdjustedClose = s.PeriodData.AdjustedClose,
                        AdjustedPercentChanged = s.PeriodData.AdjustedPercentChanged,
                        ClosingDate = s.PeriodData.ClosingDate,
                        Volume = s.PeriodData.Volume
                    }
                }).ToList()
            };
        }

        public static IEnumerable<PortfolioHistoryPeriod> FilterCopy(this IEnumerable<PortfolioHistoryPeriod> historyPeriods, List<string> symbols)
        {
            return historyPeriods.Select(p => p.FilterCopy(symbols));
        }
    }
}