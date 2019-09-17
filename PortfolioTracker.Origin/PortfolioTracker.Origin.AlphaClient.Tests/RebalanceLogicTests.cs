using System;
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
            var client = new AlphaClientWrapper(new ApiKeyProvider());
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
                    new { Symbol = "SOXL", Type = AllocationTypeEnum.Percentage, DesiredAmount = 5m },
                    new { Symbol = "CIBR", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "MJ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "TAN", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2m },
                    new { Symbol = "ICLN", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2m },
                    new { Symbol = "XBI", Type = AllocationTypeEnum.Percentage, DesiredAmount = 4m },
                    new { Symbol = "ARKG", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "XLF", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "SPY", Type = AllocationTypeEnum.Percentage, DesiredAmount = 14m },
                    new { Symbol = "QQQ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 19m },
                    new { Symbol = "DIA", Type = AllocationTypeEnum.Percentage, DesiredAmount = 12m },
                    new { Symbol = "TLT", Type = AllocationTypeEnum.Percentage, DesiredAmount = 18m },
                    new { Symbol = "VYM", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2m }
                },
                new[]
                {
                    new { Symbol = "SOXL", Type = AllocationTypeEnum.Percentage, DesiredAmount = 4m },
                    new { Symbol = "CIBR", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "MJ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "TAN", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2m },
                    new { Symbol = "ICLN", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2m },
                    new { Symbol = "XBI", Type = AllocationTypeEnum.Percentage, DesiredAmount = 4m },
                    new { Symbol = "ARKG", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "XLF", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "SPY", Type = AllocationTypeEnum.Percentage, DesiredAmount = 14m },
                    new { Symbol = "QQQ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 20m },
                    new { Symbol = "DIA", Type = AllocationTypeEnum.Percentage, DesiredAmount = 12m },
                    new { Symbol = "TLT", Type = AllocationTypeEnum.Percentage, DesiredAmount = 9m },
                    new { Symbol = "BND", Type = AllocationTypeEnum.Percentage, DesiredAmount = 9m },
                    new { Symbol = "VYM", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2m }
                },
                new[]
                {
                    new { Symbol = "SOXL", Type = AllocationTypeEnum.Percentage, DesiredAmount = 5m },
                    new { Symbol = "CIBR", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "MJ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "TAN", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2m },
                    new { Symbol = "ICLN", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2m },
                    new { Symbol = "XBI", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "ARKG", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2m },
                    new { Symbol = "XLF", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m },
                    new { Symbol = "SPY", Type = AllocationTypeEnum.Percentage, DesiredAmount = 15m },
                    new { Symbol = "QQQ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 20m },
                    new { Symbol = "DIA", Type = AllocationTypeEnum.Percentage, DesiredAmount = 12m },
                    new { Symbol = "TLT", Type = AllocationTypeEnum.Percentage, DesiredAmount = 6m },
                    new { Symbol = "SHY", Type = AllocationTypeEnum.Percentage, DesiredAmount = 6m },
                    new { Symbol = "BND", Type = AllocationTypeEnum.Percentage, DesiredAmount = 6m },
                    new { Symbol = "VYM", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2m }
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
                Console.WriteLine($"Portfolio {results.IndexOf(result)} grew by {result.PercentIncrease * 100} percent from {result.StartDate.ToShortDateString()} to {result.EndDate.ToShortDateString()}");
            }
        }

        [Test]
        public async Task Simulate()
        {
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

            var result = await _rebalanceLogic.Simulate(dataSet);

            foreach (var simulationResult in result)
            {
                var firstScenario = simulationResult.Results.First();
                var averagePercent = Math.Round(simulationResult.Results.Average(r => r.PercentIncrease), 2);
                var maxPercent = Math.Round(simulationResult.Results.Max(r => r.PercentIncrease), 2);
                var minPercent = Math.Round(simulationResult.Results.Min(r => r.PercentIncrease), 2);
                Console.WriteLine($"Portfolio {result.IndexOf(simulationResult)} experienced growth between {minPercent * 100}% and {maxPercent * 100}% with an average of {averagePercent * 100}% from {firstScenario.StartDate.ToShortDateString()} to {firstScenario.EndDate.ToShortDateString()}");
            }
        }

        [Test]
        public async Task Rebalance()
        {
            var lowRiskStocks = new Category
            {
                Name = "Low Risk Stocks",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "DIS", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 2m, CurrentShares = 1 },
                    new StockAllocation { Symbol = "SQ", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 2m, CurrentShares = 4 },
                    new StockAllocation { Symbol = "V", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 2m, CurrentShares = 1 },
                    new StockAllocation { Symbol = "MSFT", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 2m, CurrentShares = 1 }
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
                    new StockAllocation { Symbol = "MJ", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentShares = 14 }
                }
            };

            var energyEtFs = new Category
            {
                Name = "Energy",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "TAN", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentShares = 16 },
                    new StockAllocation { Symbol = "ICLN", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentShares = 32 }
                }
            };

            var bioEtFs = new Category
            {
                Name = "BioTech",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "XBI", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentShares = 4 },
                    new StockAllocation { Symbol = "ARKG", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentShares = 11 }
                }
            };

            var financeEtFs = new Category
            {
                Name = "Finance",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "XLF", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 6m, CurrentShares = 21 }
                }
            };

            var indexes = new Category
            {
                Name = "Indexes",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "SPY", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 14m, CurrentShares = 7 },
                    new StockAllocation { Symbol = "QQQ", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 14m, CurrentShares = 8 },
                    new StockAllocation { Symbol = "DIA", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 12m, CurrentShares = 5 },
                    new StockAllocation { Symbol = "VIXY", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentShares = 9 }
                }
            };

            var bonds = new Category
            {
                Name = "Bonds",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "BND", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 13m, CurrentShares = 18 },
                    new StockAllocation { Symbol = "TLT", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 4m, CurrentShares = 4 }
                }
            };

            var highRisk = new Category
            {
                Name = "High Risk Stocks",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "SLRX", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 1m, CurrentShares = 14 },
                    new StockAllocation { Symbol = "TRXC", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = .15m, CurrentShares = 24 },
                    new StockAllocation { Symbol = "EROS", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = .2m, CurrentShares = 11 },
                    new StockAllocation { Symbol = "TRNX", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = .15m, CurrentShares = 8 }
                }
            };

            Portfolio portfolio = new Portfolio
            {
                Name = "Default",
                CashOnHand = 1.00m,
                Categories = new List<Category>
                {
                    lowRiskStocks,
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
    }
}