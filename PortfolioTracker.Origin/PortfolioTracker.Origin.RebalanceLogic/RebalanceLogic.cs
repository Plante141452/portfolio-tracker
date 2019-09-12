using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PortfolioTracker.Origin.Common.Models;
using PortfolioTracker.Origin.Common.Models.Enums;
using PortfolioTracker.Origin.RebalanceLogic.Models;

namespace PortfolioTracker.Origin.RebalanceLogic
{
    public class RebalanceLogic
    {
        public async Task<List<SimulationResult>> Simulate(RunScenarioDataSet dataSet)
        {
            ConcurrentDictionary<Portfolio, List<ScenarioResult>> mapping = new ConcurrentDictionary<Portfolio, List<ScenarioResult>>();

            var results = RunScenario(dataSet);

            foreach (var scenarioResult in results)
            {
                var index = results.IndexOf(scenarioResult);
                var portfolio = dataSet.Portfolios[index];

                mapping[portfolio] = new List<ScenarioResult> { scenarioResult };
            }

            var tasks = new List<Task>();
            for (var i = 0; i < 1000; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var revisedHistory = dataSet.History.ShuffleCopy();
                    var revisedDataSet = new RunScenarioDataSet
                    {
                        CashInfluxAmount = dataSet.CashInfluxAmount,
                        CashInfluxCadence = dataSet.CashInfluxCadence,
                        History = revisedHistory,
                        InitialInvestment = dataSet.InitialInvestment,
                        Portfolios = dataSet.Portfolios
                    };

                    var nextResults = RunScenario(revisedDataSet);
                    foreach (var scenarioResult in nextResults)
                    {
                        var index = nextResults.IndexOf(scenarioResult);
                        var portfolio = dataSet.Portfolios[index];

                        mapping[portfolio].Add(scenarioResult);
                    }
                }));
            }

            await Task.WhenAll(tasks);

            return mapping.Select(kp => new SimulationResult
            {
                Portfolio = kp.Key,
                Results = kp.Value
            }).ToList();
        }

        public List<ScenarioResult> RunScenario(RunScenarioDataSet dataSet)
        {
            if (dataSet.CashInfluxCadence != CadenceTypeEnum.Weekly)
                throw new Exception($"{dataSet.CashInfluxCadence} cadence not yet supported.");

            List<ScenarioResult> results = new List<ScenarioResult>();

            foreach (var portfolio in dataSet.Portfolios)
            {
                var symbols = portfolio.Allocations.Select(a => a.Symbol).ToList();

                var currentAllocations = symbols.Select(s => new StockAllocation
                {
                    Symbol = s,
                    AllocationAmount = 0,
                    AllocationType = AllocationTypeEnum.StockAmount
                }).ToList();

                decimal cashOnHand = dataSet.InitialInvestment;
                foreach (var period in dataSet.History)
                {
                    var quotes = period.Stocks.Select(s => new Quote
                    {
                        Symbol = s.Symbol,
                        Price = s.PeriodData.AdjustedClose,
                        Time = period.ClosingDate,
                        Volume = s.PeriodData.Volume
                    }).ToList();

                    var rebalanceDataSet = new RebalanceDataSet
                    {
                        ActualAllocations = currentAllocations,
                        CashOnHand = cashOnHand,
                        Portfolio = portfolio,
                        Quotes = quotes
                    };

                    var rebalanceResult = Rebalance(rebalanceDataSet);

                    cashOnHand = rebalanceResult.RemainingCashOnHand + dataSet.CashInfluxAmount;

                    foreach (var action in rebalanceResult.Actions)
                    {
                        var actualAllocation = currentAllocations.Find(a => a.Symbol == action.Symbol);
                        if (action.ActionType == RebalanceActionTypeEnum.Buy)
                            actualAllocation.AllocationAmount += action.Amount;
                        else
                            actualAllocation.AllocationAmount -= action.Amount;
                    }
                }

                decimal finalPortfolioValue = cashOnHand - dataSet.CashInfluxAmount;
                var lastPeriod = dataSet.History.Last();

                foreach (var allocation in currentAllocations)
                    finalPortfolioValue += allocation.AllocationAmount * lastPeriod.Stocks.Find(s => s.Symbol == allocation.Symbol).PeriodData.AdjustedClose;

                var totalCashInvested = (dataSet.InitialInvestment + dataSet.CashInfluxAmount * (dataSet.History.Count - 1));
                var result = new ScenarioResult
                {
                    PercentIncrease = finalPortfolioValue / totalCashInvested,
                    StartDate = dataSet.History.First().ClosingDate,
                    EndDate = lastPeriod.ClosingDate,
                    FinalPortfolioValue = finalPortfolioValue,
                    TotalCashInvested = totalCashInvested
                };

                results.Add(result);
            }

            return results;
        }

        public RebalanceResult Rebalance(RebalanceDataSet dataSet)
        {
            if (dataSet.ActualAllocations.Any(a => a.AllocationType != AllocationTypeEnum.StockAmount))
                throw new Exception("Actual allocation amounts must be of stock amount type when rebalancing.");

            var stocks = dataSet.Portfolio.Allocations.Select(a => new RebalanceItem
            {
                DesiredAllocation = a,
                ActualAllocation = dataSet.ActualAllocations.First(aa => string.Equals(aa.Symbol, a.Symbol, StringComparison.InvariantCultureIgnoreCase)),
                Price = dataSet.Quotes.First(q => string.Equals(q.Symbol, a.Symbol, StringComparison.InvariantCultureIgnoreCase)).Price
            }).ToList();

            var portfolioValue = dataSet.CashOnHand + stocks.Sum(s => s.ActualAllocation.AllocationAmount * s.Price);

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

            return new RebalanceResult
            {
                RemainingCashOnHand = portfolioValue,
                Actions = actions
            };
        }
    }
}