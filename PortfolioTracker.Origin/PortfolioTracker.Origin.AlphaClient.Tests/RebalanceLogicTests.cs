﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PortfolioTracker.Origin.AlphaClient;
using PortfolioTracker.Origin.Common.Models;
using PortfolioTracker.Origin.Common.Models.Enums;
using PortfolioTracker.Origin.DataAccess;
using PortfolioTracker.Origin.RebalanceLogic.Models;

namespace PortfolioTracker.Origin.RebalanceLogic.Tests
{
    [TestFixture]
    public class RebalanceLogicTests
    {
        private AlphaClientLogic _alphaClient;
        private RebalanceLogic _rebalanceLogic;
        private PortfolioDataAccess _portfolioDataAccess;

        [SetUp]
        public void Setup()
        {
            var clientFactory = new AlphaVantageStocksClientFactory(new ApiKeyProvider());
            var client = new AlphaClientWrapper(clientFactory);
            var mongoWrapper = new MongoClientWrapper(new ConnectionStringProvider());
            var stockDataAccess = new StockDataAccess(mongoWrapper);
            _portfolioDataAccess = new PortfolioDataAccess(mongoWrapper);
            _alphaClient = new AlphaClientLogic(client, stockDataAccess);
            _rebalanceLogic = new RebalanceLogic();
        }

        [Test]
        public async Task Scenarios()
        {
            //SHY,TLT,IEF,LQD,AGG
            var stocks = new[]
            {
                new[]
                {
                    new { Symbol = "SOXL", Type = AllocationTypeEnum.Percentage, DesiredAmount = 6m },
                    new { Symbol = "CIBR", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "MJ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "TAN", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "ICLN", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "XBI", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "ARKG", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "XLF", Type = AllocationTypeEnum.Percentage, DesiredAmount = 6m },
                    new { Symbol = "SPY", Type = AllocationTypeEnum.Percentage, DesiredAmount = 14m },
                    new { Symbol = "QQQ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 14m },
                    new { Symbol = "DIA", Type = AllocationTypeEnum.Percentage, DesiredAmount = 12m },
                    new { Symbol = "TLT", Type = AllocationTypeEnum.Percentage, DesiredAmount = 17m },
                    new { Symbol = "VIXY", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m }
                },
                new[]
                {
                    new { Symbol = "SOXL", Type = AllocationTypeEnum.Percentage, DesiredAmount = 6m },
                    new { Symbol = "CIBR", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "MJ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "TAN", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "ICLN", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "XBI", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "ARKG", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "XLF", Type = AllocationTypeEnum.Percentage, DesiredAmount = 6m },
                    new { Symbol = "SPY", Type = AllocationTypeEnum.Percentage, DesiredAmount = 14m },
                    new { Symbol = "QQQ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 15m },
                    new { Symbol = "DIA", Type = AllocationTypeEnum.Percentage, DesiredAmount = 12m },
                    new { Symbol = "TLT", Type = AllocationTypeEnum.Percentage, DesiredAmount = 16m },
                    new { Symbol = "VIXY", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m }
                },
                new[]
                {
                    new { Symbol = "SOXL", Type = AllocationTypeEnum.Percentage, DesiredAmount = 6m },
                    new { Symbol = "CIBR", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "MJ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "TAN", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "ICLN", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "XBI", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "ARKG", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "XLF", Type = AllocationTypeEnum.Percentage, DesiredAmount = 6m },
                    new { Symbol = "SPY", Type = AllocationTypeEnum.Percentage, DesiredAmount = 14m },
                    new { Symbol = "QQQ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 15m },
                    new { Symbol = "DIA", Type = AllocationTypeEnum.Percentage, DesiredAmount = 12m },
                    new { Symbol = "TLT", Type = AllocationTypeEnum.Percentage, DesiredAmount = 16m },
                    new { Symbol = "VIXY", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m }
                }
            }.ToList();

            var symbols = stocks.SelectMany(s => s.Select(st => st.Symbol)).Distinct().ToList();
            var relevantPeriods = await _alphaClient.GetPortfolioHistory(symbols);

            var portfolios = stocks.Select(stockSet => new Portfolio
            {
                Stocks = stockSet.Select(s => new StockAllocation
                {
                    Symbol = s.Symbol,
                    DesiredAmountType = s.Type,
                    DesiredAmount = s.DesiredAmount
                }).ToList()
            }).ToList();

            RunScenarioDataSet dataSet = new RunScenarioDataSet
            {
                InitialInvestment = 10000,
                CashInfluxAmount = 150,
                CashInfluxCadence = CadenceTypeEnum.Weekly,
                Portfolios = portfolios,
                History = relevantPeriods
            };

            var results = await _rebalanceLogic.RunScenario(dataSet);

            //foreach (var action in result.Actions)
            //{
            //    Console.WriteLine($"{action.ActionType} {action.Amount} shares of {action.Symbol}");
            //}

            foreach (var result in results)
            {
                Console.WriteLine($"Portfolio {results.IndexOf(result)} grew by {result.PercentIncrease * 100} percent from {result.StartDate.Date.ToShortDateString()} to {result.EndDate.Date.ToShortDateString()}");
            }
        }

        private T Clean<T>(T category) where T : Category
        {
            category.Categories = category.Categories?.Select(Clean).ToList();
            category.Stocks = category.Stocks?.Where(s => s.DesiredAmount > 0).ToList();

            return category;
        }

        public async Task<T> OptimizeCategory<T>(T cat) where T : Category
        {
            var categorySymbols = cat.AllStocks.Select(st => st.Symbol).Distinct().ToList();
            var getCategoryPeriods = _alphaClient.GetPortfolioHistory(categorySymbols);

            var getCategories = Task.Run(async () =>
            {
                if (cat.Categories == null || !cat.Categories.Any())
                    return cat.Categories;

                var categories = new List<Category>();
                var optimizeCategories = cat.Categories.Select(OptimizeCategory);

                foreach (var optimizeCategory in optimizeCategories)
                {
                    var optimized = await optimizeCategory;

                    Clean(optimized);

                    categories.Add(optimized);
                }

                return categories;
            });

            var optimizedCategories = await getCategories;

            if (cat.Stocks == null || !cat.Stocks.Any())
            {
                cat.Categories = optimizedCategories;
                return cat;
            }

            var stockPercent = (int)cat.Stocks.Sum(s => s.DesiredAmount);
            var stockCombinations = GetPossibleStockCombinations(stockPercent, cat.Stocks.Copy(), 3, 8);

            var portfolios = stockCombinations.Select((allocations, i) => new Portfolio
            {
                Id = i.ToString(),
                Name = i.ToString(),
                CashOnHand = 0,
                Stocks = allocations.Where(a => a.DesiredAmount > 0).Copy(),
                Categories = optimizedCategories
            }).ToList();

            RunScenarioDataSet dataSet = new RunScenarioDataSet
            {
                InitialInvestment = 10000,
                CashInfluxAmount = 150,
                CashInfluxCadence = CadenceTypeEnum.Weekly,
                Portfolios = portfolios,
                History = await getCategoryPeriods
            };

            var result = await _rebalanceLogic.Simulate(dataSet);

            var winner = result.Aggregate((r1, r2) => r1.Results.Average(r => r.PercentIncrease) > r2.Results.Average(r => r.PercentIncrease) ? r1 : r2);

            cat.Categories = winner.Portfolio.Categories.Copy();
            cat.Stocks = winner.Portfolio.Stocks.Copy();

            return cat;
        }

        public List<List<StockAllocation>> GetPossibleStockCombinations(int allotedPercent, List<StockAllocation> stocks, int min, int max)
        {
            var results = new List<List<StockAllocation>>();

            if (stocks.Count == 1)
            {
                var returnStocks = stocks.Copy();
                var onlyStock = returnStocks.Single();
                onlyStock.DesiredAmount = allotedPercent;
                results.Add(returnStocks);
                return results;
            }

            var targetedStock = stocks.First();
            var remainingStocks = stocks.Skip(1).ToList();

            for (int i = min; i < max; i++)
            {
                if (remainingStocks.Count > 1)
                {
                    var innerPortfolios = GetPossibleStockCombinations(allotedPercent - i, remainingStocks, min, max);
                    if (i != 0)
                    {
                        foreach (var innerPortfolio in innerPortfolios)
                        {
                            var thisAllocation = targetedStock.Copy();
                            thisAllocation.DesiredAmount = i;

                            innerPortfolio.Add(thisAllocation);
                        }
                    }

                    results.AddRange(innerPortfolios);
                }
                else
                {
                    var thisAllocation = targetedStock.Copy();
                    thisAllocation.DesiredAmount = i;

                    var thatAllocation = remainingStocks.Single().Copy();
                    thatAllocation.DesiredAmount = allotedPercent - i;

                    var theTwo = new List<StockAllocation>();

                    if (thisAllocation.DesiredAmount != 0)
                        theTwo.Add(thisAllocation);
                    if (thatAllocation.DesiredAmount != 0)
                        theTwo.Add(thatAllocation);

                    results.Add(new List<StockAllocation> { thatAllocation, thisAllocation });
                }
            }

            //Distinct...
            return results; //.Where((allocations, i) => i == results.FindIndex(r => r.Count == allocations.Count && r.All(a1 => allocations.FirstOrDefault(a2 => a2.Symbol == a1.Symbol)?.DesiredAmount == a1.DesiredAmount))).ToList();
        }

        [Test]
        public async Task Optimize()
        {
            //Portfolio 1: 5d80d0587d2d4657d8e1fe8f
            //Portfolio 2: 5d83dbda7d2d4604505722e5
            var portfolio = await _portfolioDataAccess.GetPortfolio("5d83dbda7d2d4604505722e5");

            portfolio.Categories = portfolio.Categories.Where(c => !c.Name.Contains("Risk")).ToList();

            var symbols = portfolio.AllStocks.Select(p => p.Symbol).ToList();

            var getHistory = _alphaClient.GetPortfolioHistory(symbols);

            var result = await OptimizeCategory(portfolio);

            var portfolioSummary = string.Join("\n", result.AllStocks.Select(s => $"{s.Symbol}: {s.DesiredAmount}%"));
            Console.WriteLine(portfolioSummary);

            var finalSet = new RunScenarioDataSet
            {
                InitialInvestment = 10000,
                CashInfluxAmount = 150,
                CashInfluxCadence = CadenceTypeEnum.Weekly,
                Portfolios = new List<Portfolio> { result },
                History = await getHistory
            };

            var finalResult = await _rebalanceLogic.Simulate(finalSet);
            var simulationResult = finalResult.First();

            var percents = simulationResult.Results.Select(r => Math.Round(r.PercentIncrease, 2) * 100).OrderBy(r => r).ToList();

            var min = percents[0];
            var percentile25 = percents[(int)(.25 * percents.Count)];
            var percentile50 = percents[(int)(.50 * percents.Count)];
            var percentile75 = percents[(int)(.75 * percents.Count)];
            var max = percents.Last();

            string percentilesString = $"{min}%, {percentile25}%, {percentile50}%, {percentile75}%, {max}%";

            var firstScenario = simulationResult.Results.First();
            var averagePercent = Math.Round(percents.Average(), 2);

            //var maxPercent = Math.Round(simulationResult.Results.Max(r => r.PercentIncrease), 2);
            //var minPercent = Math.Round(simulationResult.Results.Min(r => r.PercentIncrease), 2);

            //Console.WriteLine($"Portfolio {result.IndexOf(simulationResult)} experienced growth between {minPercent * 100}% and {maxPercent * 100}% with an average of {averagePercent * 100}% from {firstScenario.StartDate.Date.ToShortDateString()} to {firstScenario.EndDate.Date.ToShortDateString()}");
            Console.WriteLine($"Optimal portfolio experienced an average growth of {averagePercent}% from {firstScenario.StartDate.Date.ToShortDateString()} to {firstScenario.EndDate.Date.ToShortDateString()} with the following percentiles: {percentilesString}.");
        }

        [Test]
        public async Task Simulate()
        {
            var stocks = new[]
            {
                new[]
                {
                    //new { Symbol = "SOXL", Type = AllocationTypeEnum.Percentage, DesiredAmount = 6m },
                    //new { Symbol = "CIBR", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    //new { Symbol = "MJ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    //new { Symbol = "TAN", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    //new { Symbol = "ICLN", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    //new { Symbol = "XBI", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    //new { Symbol = "ARKG", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    //new { Symbol = "XLF", Type = AllocationTypeEnum.Percentage, DesiredAmount = 6m },
                    new { Symbol = "SPY", Type = AllocationTypeEnum.Percentage, DesiredAmount = 16m },
                    new { Symbol = "QQQ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 16m },
                    new { Symbol = "DIA", Type = AllocationTypeEnum.Percentage, DesiredAmount = 10m },
                    new { Symbol = "BND", Type = AllocationTypeEnum.Percentage, DesiredAmount = 4m },
                    new { Symbol = "TLT", Type = AllocationTypeEnum.Percentage, DesiredAmount = 10m },
                    new { Symbol = "VIXY", Type = AllocationTypeEnum.Percentage, DesiredAmount = 4m }
                },
                new[]
                {
                    //new { Symbol = "SOXL", Type = AllocationTypeEnum.Percentage, DesiredAmount = 6m },
                    //new { Symbol = "CIBR", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    //new { Symbol = "MJ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    //new { Symbol = "TAN", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    //new { Symbol = "ICLN", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    //new { Symbol = "XBI", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    //new { Symbol = "ARKG", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    //new { Symbol = "XLF", Type = AllocationTypeEnum.Percentage, DesiredAmount = 6m },
                    new { Symbol = "SPY", Type = AllocationTypeEnum.Percentage, DesiredAmount = 16m },
                    new { Symbol = "QQQ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 16m },
                    new { Symbol = "DIA", Type = AllocationTypeEnum.Percentage, DesiredAmount = 10m },
                    new { Symbol = "BND", Type = AllocationTypeEnum.Percentage, DesiredAmount = 0m },
                    new { Symbol = "TLT", Type = AllocationTypeEnum.Percentage, DesiredAmount = 14m },
                    new { Symbol = "VIXY", Type = AllocationTypeEnum.Percentage, DesiredAmount = 4m }
                },
                new[]
                {
                    //new { Symbol = "SOXL", Type = AllocationTypeEnum.Percentage, DesiredAmount = 6m },
                    //new { Symbol = "CIBR", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    //new { Symbol = "MJ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    //new { Symbol = "TAN", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    //new { Symbol = "ICLN", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    //new { Symbol = "XBI", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    //new { Symbol = "ARKG", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    //new { Symbol = "XLF", Type = AllocationTypeEnum.Percentage, DesiredAmount = 6m },
                    new { Symbol = "SPY", Type = AllocationTypeEnum.Percentage, DesiredAmount = 16m },
                    new { Symbol = "QQQ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 16m },
                    new { Symbol = "DIA", Type = AllocationTypeEnum.Percentage, DesiredAmount = 10m },
                    new { Symbol = "BND", Type = AllocationTypeEnum.Percentage, DesiredAmount = 0m },
                    new { Symbol = "TLT", Type = AllocationTypeEnum.Percentage, DesiredAmount = 13m },
                    new { Symbol = "VIXY", Type = AllocationTypeEnum.Percentage, DesiredAmount = 5m }
                }
            }.ToList();

            var symbols = stocks.SelectMany(s => s.Select(st => st.Symbol)).Distinct().ToList();
            var relevantPeriods = await _alphaClient.GetPortfolioHistory(symbols);

            var portfolios = stocks.Select(stockSet => new Portfolio
            {
                Stocks = stockSet.Select(s => new StockAllocation
                {
                    Symbol = s.Symbol,
                    DesiredAmountType = s.Type,
                    DesiredAmount = s.DesiredAmount
                }).ToList()
            }).ToList();

            RunScenarioDataSet dataSet = new RunScenarioDataSet
            {
                InitialInvestment = 10000,
                CashInfluxAmount = 150,
                CashInfluxCadence = CadenceTypeEnum.Weekly,
                Portfolios = portfolios,
                History = relevantPeriods
            };

            var result = await _rebalanceLogic.Simulate(dataSet);

            foreach (var simulationResult in result)
            {
                var percents = simulationResult.Results.Select(r => Math.Round(r.PercentIncrease, 2) * 100).OrderBy(r => r).ToList();

                var min = percents[0];
                var percentile25 = percents[(int)(.25 * percents.Count)];
                var percentile50 = percents[(int)(.50 * percents.Count)];
                var percentile75 = percents[(int)(.75 * percents.Count)];
                var max = percents.Last();

                string percentilesString = $"{min}%, {percentile25}%, {percentile50}%, {percentile75}%, {max}%";

                var firstScenario = simulationResult.Results.First();
                var averagePercent = Math.Round(percents.Average(), 2);

                //var maxPercent = Math.Round(simulationResult.Results.Max(r => r.PercentIncrease), 2);
                //var minPercent = Math.Round(simulationResult.Results.Min(r => r.PercentIncrease), 2);

                //Console.WriteLine($"Portfolio {result.IndexOf(simulationResult)} experienced growth between {minPercent * 100}% and {maxPercent * 100}% with an average of {averagePercent * 100}% from {firstScenario.StartDate.Date.ToShortDateString()} to {firstScenario.EndDate.Date.ToShortDateString()}");
                Console.WriteLine($"Portfolio {result.IndexOf(simulationResult)} experienced an average growth of {averagePercent}% from {firstScenario.StartDate.Date.ToShortDateString()} to {firstScenario.EndDate.Date.ToShortDateString()} with the following percentiles: {percentilesString}.");
            }
        }

        [Test]
        public async Task Rebalance()
        {
            var portfolio = await _portfolioDataAccess.GetPortfolio("5d80d0587d2d4657d8e1fe8f");
            var symbols = portfolio.AllStocks.Select(s => s.Symbol).ToList();
            var quotes = await _alphaClient.GetQuotes(symbols);

            RebalanceDataSet dataSet = new RebalanceDataSet
            {
                Portfolio = portfolio,
                Quotes = quotes
            };

            var result = _rebalanceLogic.Rebalance(dataSet);

            foreach (var action in result.Actions)
            {
                Console.WriteLine($"{action.ActionType} {action.Amount} shares of {action.Symbol}");
            }

            Console.WriteLine($"${result.RemainingCashOnHand} remaining");
        }

        [Test]
        public async Task UpdatePortfolio()
        {
            var lowRiskStocks = new Category
            {
                Name = "Low Risk Stocks",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "DIS", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 2m, CurrentShares = 2 },
                    new StockAllocation { Symbol = "MSFT", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 2m, CurrentShares = 2 },
                    new StockAllocation { Symbol = "V", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 2m, CurrentShares = 1 }
                }
            };
            var mediumRiskStocks = new Category
            {
                Name = "Medium Risk Stocks",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "SQ", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 2m, CurrentShares = 4 },
                    //new StockAllocation { Symbol = "ROKU", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 1.5m, CurrentShares = 0 }
                }
            };

            var techEtFs = new Category
            {
                Name = "Tech",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "SOXL", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 6m, CurrentShares = 4 },
                    new StockAllocation { Symbol = "CIBR", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentShares = 13 }
                }
            };

            var cannabisEtFs = new Category
            {
                Name = "Cannabis",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "MJ", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentShares = 16 }
                }
            };

            var energyEtFs = new Category
            {
                Name = "Energy",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "TAN", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentShares = 10 },
                    new StockAllocation { Symbol = "ICLN", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentShares = 30 }
                }
            };

            var bioEtFs = new Category
            {
                Name = "BioTech",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "XBI", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentShares = 4 },
                    new StockAllocation { Symbol = "ARKG", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentShares = 10 }
                }
            };

            var financeEtFs = new Category
            {
                Name = "Finance",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "XLF", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 6m, CurrentShares = 25 }
                }
            };

            var indexes = new Category
            {
                Name = "Indexes",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "SPY", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 16m, CurrentShares = 7 },
                    new StockAllocation { Symbol = "QQQ", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 16m, CurrentShares = 10 },
                    new StockAllocation { Symbol = "DIA", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 10m, CurrentShares = 4 }
                }
            };

            var bonds = new Category
            {
                Name = "Bonds",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "BND", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 4m, CurrentShares = 5 },
                    new StockAllocation { Symbol = "TLT", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 10m, CurrentShares = 8 },
                    new StockAllocation { Symbol = "VIXY", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 4m, CurrentShares = 20 }
                }
            };

            var highRisk = new Category
            {
                Name = "High Risk Stocks",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "SLRX", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = .5m, CurrentShares = 15 },
                    new StockAllocation { Symbol = "TRXC", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = .15m, CurrentShares = 26 },
                    new StockAllocation { Symbol = "EROS", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = .2m, CurrentShares = 7 },
                    new StockAllocation { Symbol = "TRNX", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = .15m, CurrentShares = 10 }
                }
            };

            Portfolio portfolio = new Portfolio
            {
                Id = "5d80d0587d2d4657d8e1fe8f",
                Name = "Default",
                CashOnHand = 04,
                Categories = new List<Category>
                {
                    lowRiskStocks,
                    mediumRiskStocks,
                    new Category
                    {
                        Name = "ETFs",
                        Categories = new List<Category>
                        {
                            techEtFs,
                            cannabisEtFs,
                            energyEtFs,
                            bioEtFs,
                            financeEtFs
                        }
                    },
                    indexes,
                    bonds,
                    highRisk
                }
            };

            var result = await _portfolioDataAccess.SavePortfolios(new List<Portfolio> { portfolio });
            Console.WriteLine($"Portfolio Id: {result.First().Id}");
        }

        [Test]
        public async Task UpdatePortfolio2()
        {
            var etfs = new Category
            {
                Name = "Etfs",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "SOXL", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 20m, CurrentShares = 4 },
                    new StockAllocation { Symbol = "CIBR", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 0m, CurrentShares = 13 },
                    //new StockAllocation { Symbol = "MJ", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 0m, CurrentShares = 14 },
                    new StockAllocation { Symbol = "TAN", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 0m, CurrentShares = 11 },
                    new StockAllocation { Symbol = "ICLN", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 0m, CurrentShares = 31 },
                    new StockAllocation { Symbol = "XBI", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 0m, CurrentShares = 4 },
                    new StockAllocation { Symbol = "ARKG", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 0m, CurrentShares = 11 },
                    new StockAllocation { Symbol = "XLF", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 0m, CurrentShares = 23 }
                }
            };
            //10
            //25
            //65
            //var stable = new Category
            //{
            //    Name = "Stable",
            //    Stocks = new List<StockAllocation>
            //    {
            //        new StockAllocation { Symbol = "SPY", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 15m, CurrentShares = 7 },
            //        new StockAllocation { Symbol = "QQQ", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 20m, CurrentShares = 8 },
            //        new StockAllocation { Symbol = "DIA", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 10m, CurrentShares = 5 },
            //        new StockAllocation { Symbol = "VIXY", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 5m, CurrentShares = 14 },
            //        //new StockAllocation { Symbol = "BND", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 10m, CurrentShares = 18 },
            //        //new StockAllocation { Symbol = "TLT", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 5m, CurrentShares = 4 }
            //    }
            //};

            Portfolio portfolio = new Portfolio
            {
                Id = "5d83dbda7d2d4604505722e5",
                Name = "Default",
                CashOnHand = 1.37m,
                Categories = new List<Category>
                {
                    etfs,
                    //stable
                }
            };

            var result = await _portfolioDataAccess.SavePortfolios(new List<Portfolio> { portfolio });
            Console.WriteLine($"Portfolio Id: {result.First().Id}");
        }
    }
}