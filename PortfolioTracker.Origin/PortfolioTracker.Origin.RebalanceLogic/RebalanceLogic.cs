using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PortfolioTracker.Origin.Common.Models;
using PortfolioTracker.Origin.Common.Models.Enums;
using PortfolioTracker.Origin.RebalanceLogic.Interfaces;
using PortfolioTracker.Origin.RebalanceLogic.Models;

namespace PortfolioTracker.Origin.RebalanceLogic
{
    public class RebalanceLogic : IRebalanceLogic
    {
        public async Task<List<SimulationResult>> Simulate(RunScenarioDataSet dataSet)
        {
            for (int d = 0; d < dataSet.Portfolios.Count; d++)
            {
                dataSet.Portfolios[d].Id = d.ToString();
            }

            ConcurrentDictionary<string, ConcurrentQueue<ScenarioResult>> mapping = new ConcurrentDictionary<string, ConcurrentQueue<ScenarioResult>>();

            var results = await RunScenario(dataSet);

            foreach (var scenarioResult in results)
            {
                var index = results.IndexOf(scenarioResult);
                var portfolio = dataSet.Portfolios[index].Copy();

                mapping[portfolio.Id] = new ConcurrentQueue<ScenarioResult>();
                mapping[portfolio.Id].Enqueue(scenarioResult);
            }

            var tasks = new List<Task>();

            int stockCount = (int)dataSet.Portfolios.Average(p => p.AllStocks.Count);
            int iterationCount = 4000000 / (dataSet.Portfolios.Count * dataSet.History.Count);

            var start = DateTime.Now;

            for (var i = 0; i < iterationCount; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var revisedHistory = dataSet.History.ShuffleCopy();
                    var revisedDataSet = new RunScenarioDataSet
                    {
                        CashInfluxAmount = dataSet.CashInfluxAmount,
                        CashInfluxCadence = dataSet.CashInfluxCadence,
                        History = revisedHistory,
                        InitialInvestment = dataSet.InitialInvestment,
                        Portfolios = dataSet.Portfolios.Copy()
                    };

                    var nextResults = await RunScenario(revisedDataSet);
                    foreach (var scenarioResult in nextResults)
                    {
                        var index = nextResults.IndexOf(scenarioResult);
                        var portfolio = dataSet.Portfolios[index];

                        mapping[portfolio.Id].Enqueue(scenarioResult);
                    }
                }));
            }

            await Task.WhenAll(tasks);

            var duration = DateTime.Now.Subtract(start).TotalSeconds;
            Console.WriteLine($"{dataSet.Portfolios.Count} portfolios, {dataSet.History.Count} weeks, {stockCount} stocks took {duration}s");

            return mapping.Select(kp => new SimulationResult
            {
                Portfolio = dataSet.Portfolios.First(p => kp.Key == p.Id),
                Results = kp.Value.ToList()
            }).ToList();
        }

        public async Task<List<ScenarioResult>> RunScenario(RunScenarioDataSet dataSet)
        {
            if (dataSet.CashInfluxCadence != CadenceTypeEnum.Weekly)
                throw new Exception($"{dataSet.CashInfluxCadence} cadence not yet supported.");

            ConcurrentQueue<ScenarioResult> results = new ConcurrentQueue<ScenarioResult>();

            await Task.WhenAll(dataSet.Portfolios.Select(p => Task.Run(() =>
            {
                var portfolio = p.Copy();

                var symbols = portfolio.AllStocks.Select(a => a.Symbol).ToList();

                var currentAllocations = symbols.Select(s => new StockAllocation
                {
                    Symbol = s,
                    DesiredAmount = 0,
                    DesiredAmountType = AllocationTypeEnum.StockAmount
                }).ToList();

                decimal cashOnHand = dataSet.InitialInvestment;
                foreach (var period in dataSet.History)
                {
                    var quotes = period.Stocks.Select(s => new Quote
                    {
                        Symbol = s.Symbol,
                        Price = s.PeriodData.AdjustedClose,
                        QuoteDate = period.ClosingDate,
                        Volume = s.PeriodData.Volume
                    }).ToList();

                    portfolio.CashOnHand = cashOnHand;
                    foreach (var allocation in portfolio.AllStocks)
                    {
                        var currentAllocation = currentAllocations.Find(s => s.Symbol == allocation.Symbol);
                        allocation.CurrentShares = currentAllocation.CurrentShares;
                    }

                    var rebalanceDataSet = new RebalanceDataSet
                    {
                        Portfolio = portfolio,
                        Quotes = quotes
                    };

                    var rebalanceResult = Rebalance(rebalanceDataSet);

                    cashOnHand = rebalanceResult.RemainingCashOnHand + dataSet.CashInfluxAmount;

                    foreach (var action in rebalanceResult.Actions)
                    {
                        var actualAllocation = currentAllocations.Find(a => a.Symbol == action.Symbol);
                        if (action.ActionType == RebalanceActionTypeEnum.Buy)
                            actualAllocation.CurrentShares += action.Amount;
                        else
                            actualAllocation.CurrentShares -= action.Amount;
                    }
                }

                decimal finalPortfolioValue = cashOnHand - dataSet.CashInfluxAmount;
                var lastPeriod = dataSet.History.Last();

                foreach (var allocation in currentAllocations)
                    finalPortfolioValue += allocation.CurrentShares * lastPeriod.Stocks.Find(s => s.Symbol == allocation.Symbol).PeriodData.AdjustedClose;

                var totalCashInvested = (dataSet.InitialInvestment + dataSet.CashInfluxAmount * (dataSet.History.Count - 1));
                var result = new ScenarioResult
                {
                    PercentIncrease = (finalPortfolioValue / totalCashInvested) - 1,
                    StartDate = dataSet.History.First().ClosingDate,
                    EndDate = lastPeriod.ClosingDate,
                    FinalPortfolioValue = finalPortfolioValue,
                    TotalCashInvested = totalCashInvested
                };

                results.Enqueue(result);
            })));

            return results.ToList();
        }

        public RebalanceResult Rebalance(RebalanceDataSet dataSet)
        {
            var stocks = dataSet.Portfolio.AllStocks.Select(a => new RebalanceItem
            {
                Symbol = a.Symbol,
                CurrentShares = a.CurrentShares,
                DesiredAmountType = a.DesiredAmountType,
                DesiredAmount = a.DesiredAmount,
                Price = dataSet.Quotes.First(q => string.Equals(q.Symbol, a.Symbol, StringComparison.InvariantCultureIgnoreCase)).Price
            }).ToList();

            var portfolioValue = dataSet.Portfolio.CashOnHand + stocks.Sum(s => s.CurrentShares * s.Price);

            var amountStocks = stocks.Where(s => s.DesiredAmountType == AllocationTypeEnum.StockAmount).ToList();
            var cashStocks = stocks.Where(s => s.DesiredAmountType == AllocationTypeEnum.CashAmount).ToList();

            List<RebalanceAction> actions = new List<RebalanceAction>();

            foreach (var stock in amountStocks)
            {
                if (stock.CurrentShares != stock.DesiredAmount)
                {
                    var dif = (int)stock.DesiredAmount - stock.CurrentShares;
                    var action = dif > 0 ? RebalanceActionTypeEnum.Buy : RebalanceActionTypeEnum.Sell;

                    actions.Add(new RebalanceAction
                    {
                        Symbol = stock.Symbol,
                        ActionType = action,
                        Amount = Math.Abs(dif)
                    });
                }

                portfolioValue -= stock.DesiredAmount * stock.Price;
            }

            foreach (var stock in cashStocks)
            {
                var actualValue = stock.CurrentShares * stock.Price;
                var dif = stock.DesiredAmount - actualValue; // (-100)

                var difAmount = (int)Math.Truncate(dif / stock.Price); // 2 (-45)
                if (difAmount > 0)
                    difAmount -= 1; //Stay below max amount

                portfolioValue -= (stock.CurrentShares + dif) * stock.Price;

                if (difAmount != 0)
                {
                    var action = difAmount > 0 ? RebalanceActionTypeEnum.Buy : RebalanceActionTypeEnum.Sell;

                    actions.Add(new RebalanceAction
                    {
                        Symbol = stock.Symbol,
                        ActionType = action,
                        Amount = Math.Abs(difAmount)
                    });
                }
            }

            var percentageStocks = stocks.Where(s => s.DesiredAmountType == AllocationTypeEnum.Percentage).ToList();
            var idealTotalFactor = percentageStocks.Sum(s => s.DesiredAmount);

            var factors = percentageStocks.Select(s => new
            {
                s.Symbol,
                s.CurrentShares,
                s.DesiredAmount,
                s.DesiredAmountType,
                s.Price,
                DesiredFactor = s.DesiredAmount / idealTotalFactor,
                EndAmount = (int)Math.Truncate((((s.DesiredAmount / idealTotalFactor) * portfolioValue) / s.Price))
            }).Select(s => new RebalancePercentageItem
            {
                Symbol = s.Symbol,
                CurrentShares = s.CurrentShares,
                DesiredAmount = s.DesiredAmount,
                DesiredAmountType = s.DesiredAmountType,
                Price = s.Price,
                DesiredFactor = s.DesiredFactor,
                CurrentFactor = (s.EndAmount * s.Price) / portfolioValue,
                EndAmount = s.EndAmount
            }).ToList();

            var usedUpAmount = factors.Sum(f => f.Price * f.EndAmount);

            while (true)
            {
                var amountRemaining = portfolioValue - usedUpAmount;

                var mostExpensiveFactor = factors.Where(f => f.Price < amountRemaining).OrderByDescending(f => f.FactorDifference).FirstOrDefault();
                if (mostExpensiveFactor == null)
                    break;

                var adjustedFactor = ((mostExpensiveFactor.EndAmount + 1) * mostExpensiveFactor.Price) / portfolioValue;

                mostExpensiveFactor.EndAmount += 1;
                mostExpensiveFactor.CurrentFactor = adjustedFactor;
                usedUpAmount += mostExpensiveFactor.Price;

                //var factorsInPlay = factors.Where(f => f.Price < amountRemaining).OrderByDescending(f => f.FactorDifference).ToList();
                //if (!factorsInPlay.Any())
                //{
                //    break;
                //}
                //
                //bool filled = false;
                //foreach (var factor in factorsInPlay)
                //{
                //    var adjustedFactor = ((factor.EndAmount + 1) * factor.Price) / portfolioValue;
                //    var newDifference = Math.Abs(factor.DesiredFactor - adjustedFactor);
                //    if (factorsInPlay.Count == 1 || factorsInPlay.Any(f => f.FactorDifference > newDifference))
                //    {
                //        factor.EndAmount += 1;
                //        factor.CurrentFactor = adjustedFactor;
                //        filled = true;
                //        break;
                //    }
                //}
                //
                //if (!filled)
                //    break;
            }

            actions.AddRange(factors.Where(f => f.EndAmount != f.CurrentShares).Select(f => new RebalanceAction
            {
                Symbol = f.Symbol,
                ActionType = f.EndAmount > f.CurrentShares ? RebalanceActionTypeEnum.Buy : RebalanceActionTypeEnum.Sell,
                Amount = Math.Abs(f.EndAmount - f.CurrentShares)
            }));

            portfolioValue -= factors.Sum(f => f.Price * f.EndAmount);

            if (portfolioValue < 0)
                throw new Exception("Dataset resulted in a negative balance");

            return new RebalanceResult
            {
                RemainingCashOnHand = portfolioValue,
                Actions = actions
            };
        }
    }
}