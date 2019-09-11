using AlphaVantage.Net.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortfolioTracker.Origin.AlphaClient
{
    public class ApiKeyProvider : IApiKeyProvider
    {
        public string AlphaVantageKey => "6SJAMSWRHH1TB6R7";
    }

    public class AlphaClient
    {
        private readonly Lazy<AlphaVantageStocksClient> _clientLazy;
        private readonly IApiKeyProvider _apiKeyProvider;

        private AlphaVantageStocksClient Client => _clientLazy.Value;

        public AlphaClient(IApiKeyProvider apiKeyProvider)
        {
            this._apiKeyProvider = apiKeyProvider;
            _clientLazy = new Lazy<AlphaVantageStocksClient>(() => new AlphaVantageStocksClient(_apiKeyProvider.AlphaVantageKey));
        }

        public async Task<RebalanceResult> Rebalance(RebalanceDataSet dataSet)
        {
            if (dataSet.ActualAllocations.Any(a => a.AllocationType != AllocationTypeEnum.StockAmount))
                throw new Exception("Actual allocation amounts must be of stock amount type when rebalancing.");

            var symbols = dataSet.Portfolio.Allocations.Select(a => a.Symbol).ToArray();

            var quotes = await Client.RequestBatchQuotesAsync(symbols);

            var portfolioValue = dataSet.CashOnHand;

            var stocks = dataSet.Portfolio.Allocations.Select(a => new RebalanceItem
            {
                DesiredAllocation = a,
                ActualAllocation = dataSet.ActualAllocations.First(aa => string.Equals(aa.Symbol, a.Symbol, StringComparison.InvariantCultureIgnoreCase)),
                Price = quotes.First(q => string.Equals(q.Symbol, a.Symbol, StringComparison.InvariantCultureIgnoreCase)).Price
            }).ToList();

            portfolioValue = dataSet.CashOnHand + stocks.Sum(s => s.ActualAllocation.AllocationAmount * s.Price);

            var amountStocks = stocks.Where(s => s.DesiredAllocation.AllocationType == AllocationTypeEnum.StockAmount).ToList();
            var cashStocks = stocks.Where(s => s.DesiredAllocation.AllocationType == AllocationTypeEnum.CashAmount).ToList();

            List<RebalanceAction> actions = new List<RebalanceAction>();

            foreach (var stock in amountStocks)
            {
                if (stock.ActualAllocation.AllocationAmount != stock.DesiredAllocation.AllocationAmount)
                {
                    var dif = (int)stock.DesiredAllocation.AllocationAmount - (int)stock.ActualAllocation.AllocationAmount;
                    var action = dif > 0 ? RebalanceActionTypeEnum.Buy : RebalanceActionTypeEnum.Sell;

                    actions.Add(new RebalanceAction
                    {
                        Symbol = stock.DesiredAllocation.Symbol,
                        ActionType = action,
                        Amount = Math.Abs(dif)
                    });
                }

                portfolioValue -= stock.DesiredAllocation.AllocationAmount * stock.Price;
            }

            foreach (var stock in cashStocks)
            {
                var actualValue = stock.ActualAllocation.AllocationAmount * stock.Price;
                var dif = stock.DesiredAllocation.AllocationAmount - actualValue; // (-100)

                var difAmount = (int)Math.Truncate(dif / stock.Price); // 2 (-45)
                if (difAmount > 0)
                    difAmount -= 1; //Stay below max amount

                portfolioValue -= (stock.ActualAllocation.AllocationAmount + dif) * stock.Price;

                if (difAmount != 0)
                {
                    var action = difAmount > 0 ? RebalanceActionTypeEnum.Buy : RebalanceActionTypeEnum.Sell;

                    actions.Add(new RebalanceAction
                    {
                        Symbol = stock.DesiredAllocation.Symbol,
                        ActionType = action,
                        Amount = Math.Abs(difAmount)
                    });
                }

            }

            var percentageStocks = stocks.Where(s => s.DesiredAllocation.AllocationType == AllocationTypeEnum.Percentage).ToList();
            var idealTotalFactor = percentageStocks.Sum(s => s.DesiredAllocation.AllocationAmount);
            var currentTotalAmount = percentageStocks.Sum(s => s.ActualAllocation.AllocationAmount * s.Price);

            var factors = percentageStocks.Select(s => new
            {
                s.ActualAllocation,
                s.DesiredAllocation,
                s.Price,
                DesiredFactor = s.DesiredAllocation.AllocationAmount / idealTotalFactor,
                EndAmount = (int)Math.Truncate((((s.DesiredAllocation.AllocationAmount / idealTotalFactor) * portfolioValue) / s.Price))
            }).Select(s => new RebalancePercentageItem
            {
                ActualAllocation = s.ActualAllocation,
                DesiredAllocation = s.DesiredAllocation,
                Price = s.Price,
                DesiredFactor = s.DesiredFactor,
                CurrentFactor = (s.EndAmount * s.Price) / portfolioValue,
                EndAmount = s.EndAmount
            }).ToList();

            while (true)
            {
                var usedUpAmount = factors.Sum(f => f.Price * f.EndAmount);
                var amountRemaining = portfolioValue - usedUpAmount;

                var factorsInPlay = factors.Where(f => f.Price < amountRemaining).OrderByDescending(f => f.FactorDifference).ToList();
                if (!factorsInPlay.Any())
                {
                    break;
                }

                bool filled = false;
                foreach (var factor in factorsInPlay)
                {
                    var adjustedFactor = ((factor.EndAmount + 1) * factor.Price) / portfolioValue;
                    var newDifference = Math.Abs(factor.DesiredFactor - adjustedFactor);
                    if (factorsInPlay.Count == 1 || factorsInPlay.Any(f => f.FactorDifference > newDifference))
                    {
                        factor.EndAmount += 1;
                        factor.CurrentFactor = adjustedFactor;
                        filled = true;
                        break;
                    }
                }

                if (!filled)
                    break;
            }

            actions.AddRange(factors.Where(f => f.EndAmount != f.ActualAllocation.AllocationAmount).Select(f => new RebalanceAction
            {
                Symbol = f.DesiredAllocation.Symbol,
                ActionType = f.EndAmount > f.ActualAllocation.AllocationAmount ? RebalanceActionTypeEnum.Buy : RebalanceActionTypeEnum.Sell,
                Amount = (int)Math.Abs(f.EndAmount - f.ActualAllocation.AllocationAmount)
            }));

            portfolioValue -= factors.Sum(f => f.Price * f.EndAmount);

            if (portfolioValue < 0)
                throw new Exception("Dataset resulted in a negative balance");

            return new RebalanceResult { Actions = actions };
        }
    }

    public class RebalancePercentageItem : RebalanceItem
    {
        public decimal DesiredFactor { get; set; }
        public decimal CurrentFactor { get; set; }
        public int EndAmount { get; set; }
        public decimal FactorDifference => Math.Abs(DesiredFactor - CurrentFactor);
    }

    public class RebalanceItem
    {
        public StockAllocation ActualAllocation { get; set; }
        public StockAllocation DesiredAllocation { get; set; }
        public decimal Price { get; set; }
    }

    public enum AllocationTypeEnum
    {
        Percentage,
        CashAmount,
        StockAmount
    }

    public class StockAllocation
    {
        public string Symbol { get; set; }
        public AllocationTypeEnum AllocationType { get; set; }
        public decimal AllocationAmount { get; set; }
    }

    public class Portfolio
    {
        public List<StockAllocation> Allocations { get; set; }
    }

    public enum RebalanceActionTypeEnum
    {
        Buy,
        Sell
    }

    public class RebalanceAction
    {
        public string Symbol { get; set; }
        public RebalanceActionTypeEnum ActionType { get; set; }
        public int Amount { get; set; }
    }

    public class RebalanceResult
    {
        public List<RebalanceAction> Actions { get; set; }
    }

    public class RebalanceDataSet
    {
        public Portfolio Portfolio { get; set; }
        public List<StockAllocation> ActualAllocations { get; set; }
        public decimal CashOnHand { get; set; }
    }
}
