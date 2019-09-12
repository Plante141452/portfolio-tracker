using System;
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

        [SetUp]
        public void Setup()
        {
            var client = new AlphaClientWrapper(new ApiKeyProvider());
            var stockDataAccess = new StockDataAccess(new MongoClientWrapper(new ConnectionStringProvider()));
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
                Allocations = stockSet.Select(s => new StockAllocation
                {
                    Symbol = s.Symbol,
                    AllocationType = s.Type,
                    AllocationAmount = s.DesiredAmount
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
                    new { Symbol = "TLT", Type = AllocationTypeEnum.Percentage, DesiredAmount = 18m },
                    new { Symbol = "VYM", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2m }
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
                    new { Symbol = "QQQ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 14m },
                    new { Symbol = "DIA", Type = AllocationTypeEnum.Percentage, DesiredAmount = 12m },
                    new { Symbol = "TLT", Type = AllocationTypeEnum.Percentage, DesiredAmount = 18m },
                    new { Symbol = "VYM", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2m }
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
                    new { Symbol = "QQQ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 14m },
                    new { Symbol = "DIA", Type = AllocationTypeEnum.Percentage, DesiredAmount = 12m },
                    new { Symbol = "TLT", Type = AllocationTypeEnum.Percentage, DesiredAmount = 18m },
                    new { Symbol = "VYM", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2m }
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
                    new { Symbol = "QQQ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 14m },
                    new { Symbol = "DIA", Type = AllocationTypeEnum.Percentage, DesiredAmount = 12m },
                    new { Symbol = "TLT", Type = AllocationTypeEnum.Percentage, DesiredAmount = 18m },
                    new { Symbol = "VYM", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2m }
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
                    new { Symbol = "QQQ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 14m },
                    new { Symbol = "DIA", Type = AllocationTypeEnum.Percentage, DesiredAmount = 12m },
                    new { Symbol = "TLT", Type = AllocationTypeEnum.Percentage, DesiredAmount = 18m },
                    new { Symbol = "VYM", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2m }
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
                    new { Symbol = "QQQ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 14m },
                    new { Symbol = "DIA", Type = AllocationTypeEnum.Percentage, DesiredAmount = 12m },
                    new { Symbol = "TLT", Type = AllocationTypeEnum.Percentage, DesiredAmount = 18m },
                    new { Symbol = "VYM", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2m }
                }
            }.ToList();

            var symbols = stocks.SelectMany(s => s.Select(st => st.Symbol)).Distinct().ToList();
            var relevantPeriods = await _alphaClient.GetPortfolioHistory(symbols);

            var portfolios = stocks.Select(stockSet => new Portfolio
            {
                Allocations = stockSet.Select(s => new StockAllocation
                {
                    Symbol = s.Symbol,
                    AllocationType = s.Type,
                    AllocationAmount = s.DesiredAmount
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
            var stocks = new[]
            {
                new { Symbol = "DIS", Type = AllocationTypeEnum.StockAmount, DesiredAmount = 1m, CurrentAmount = 1 },
                new { Symbol = "SLRX", Type = AllocationTypeEnum.StockAmount, DesiredAmount = 10m, CurrentAmount = 10 },
                new { Symbol = "SQ", Type = AllocationTypeEnum.StockAmount, DesiredAmount = 2m, CurrentAmount = 2 },
                new { Symbol = "V", Type = AllocationTypeEnum.StockAmount, DesiredAmount = 1m, CurrentAmount = 1 },
                new { Symbol = "MSFT", Type = AllocationTypeEnum.StockAmount, DesiredAmount = 1m, CurrentAmount = 1 },
                new { Symbol = "TRXC", Type = AllocationTypeEnum.StockAmount, DesiredAmount = 15m, CurrentAmount = 15 },
                new { Symbol = "EROS", Type = AllocationTypeEnum.StockAmount, DesiredAmount = 2m, CurrentAmount = 2 },
                new { Symbol = "TRNX", Type = AllocationTypeEnum.StockAmount, DesiredAmount = 5m, CurrentAmount = 5 },
                new { Symbol = "SOXL", Type = AllocationTypeEnum.Percentage, DesiredAmount = 6m, CurrentAmount = 3 },
                new { Symbol = "CIBR", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentAmount = 13 },
                new { Symbol = "MJ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentAmount = 12 },
                new { Symbol = "TAN", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentAmount = 16 },
                new { Symbol = "ICLN", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentAmount = 40 },
                new { Symbol = "XBI", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentAmount = 4 },
                new { Symbol = "ARKG", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentAmount = 10 },
                new { Symbol = "XLF", Type = AllocationTypeEnum.Percentage, DesiredAmount = 6m, CurrentAmount = 13 },
                new { Symbol = "SPY", Type = AllocationTypeEnum.Percentage, DesiredAmount = 14m, CurrentAmount = 6 },
                new { Symbol = "QQQ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 14m, CurrentAmount = 11 },
                new { Symbol = "DIA", Type = AllocationTypeEnum.Percentage, DesiredAmount = 12m, CurrentAmount = 5 },
                new { Symbol = "BND", Type = AllocationTypeEnum.Percentage, DesiredAmount = 16m, CurrentAmount = 25 },
                new { Symbol = "TLT", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2m, CurrentAmount = 0 },
                new { Symbol = "VYM", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2m, CurrentAmount = 3 },
            };

            var actualAllocations = stocks.Select(s => new StockAllocation
            {
                Symbol = s.Symbol,
                AllocationType = AllocationTypeEnum.StockAmount,
                AllocationAmount = s.CurrentAmount
            }).ToList();

            var portfolio = new Portfolio
            {
                Allocations = stocks.Select(s => new StockAllocation
                {
                    Symbol = s.Symbol,
                    AllocationType = s.Type,
                    AllocationAmount = s.DesiredAmount
                }).ToList()
            };

            var symbols = stocks.Select(s => s.Symbol).ToList();
            var quotes = await _alphaClient.GetQuotes(symbols);

            RebalanceDataSet dataSet = new RebalanceDataSet
            {
                CashOnHand = 32.54m,
                ActualAllocations = actualAllocations,
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