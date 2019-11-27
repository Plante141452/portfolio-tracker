using System;
using System.Collections.Generic;
using System.Linq;
using PortfolioTracker.Origin.Common.Models;

namespace PortfolioTracker.Origin.RebalanceLogic
{
    public static class SimulationExtensions
    {
        public static List<PortfolioHistoryPeriod> ShuffleCopy(this IList<PortfolioHistoryPeriod> list, int? seed = null)
        {
            Random rng = seed.HasValue ? new Random(seed.Value) : new Random();

            var shuffled = list.Select(l => new PortfolioHistoryPeriod
            {
                ClosingDate = l.ClosingDate,
                Stocks = l.Stocks.Select(s => new StockHistoricalPeriod
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
            }).ToList();

            int n = shuffled.Count - 1;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);

                var originalKDate = shuffled[k].ClosingDate;

                PortfolioHistoryPeriod value = shuffled[k];

                value.ClosingDate = shuffled[n].ClosingDate;
                shuffled[n].ClosingDate = originalKDate;

                shuffled[k] = shuffled[n];
                shuffled[n] = value;
            }

            shuffled = shuffled.OrderByDescending(p => p.ClosingDate).ToList();

            PortfolioHistoryPeriod lastPeriod = shuffled.First();
            foreach (var period in shuffled.Skip(1))
            {
                foreach (var stock in period.Stocks)
                {
                    var lastStockPeriodData = lastPeriod.Stocks.First(s => s.Symbol == stock.Symbol).PeriodData;
                    if (lastStockPeriodData.AdjustedPercentChanged > .0001m)
                        stock.PeriodData.AdjustedClose = lastStockPeriodData.AdjustedClose / lastStockPeriodData.AdjustedPercentChanged;
                    else
                        stock.PeriodData.AdjustedClose = lastStockPeriodData.AdjustedClose;
                }

                lastPeriod = period;
            }

            var adjusted = shuffled.OrderBy(p => p.ClosingDate).ToList();
            return adjusted;
        }
    }
}