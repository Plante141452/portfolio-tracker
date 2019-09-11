using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PortfolioTracker.Origin.AlphaClient;
using PortfolioTracker.Origin.Common.Models;
using PortfolioTracker.Origin.Common.Models.Enums;
using PortfolioTracker.Origin.RebalanceLogic.Models;

namespace PortfolioTracker.Origin.RebalanceLogic.Tests
{
    [TestFixture]
    public class RebalanceLogicTests
    {
        private AlphaClient.AlphaClient _alphaClient;
        private RebalanceLogic _rebalanceLogic;

        [SetUp]
        public void Setup()
        {
            _alphaClient = new AlphaClient.AlphaClient(new ApiKeyProvider());
            _rebalanceLogic = new RebalanceLogic();
        }

        [Test]
        public async Task Scenario()
        {
            var stocks = new[]
            {
                new { Symbol = "SPY", Type = AllocationTypeEnum.Percentage, DesiredAmount = 1 },
                new { Symbol = "QQQ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 10 },
                new { Symbol = "DIA", Type = AllocationTypeEnum.Percentage, DesiredAmount = 1 },
                new { Symbol = "BND", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3 },
                new { Symbol = "VYM", Type = AllocationTypeEnum.Percentage, DesiredAmount = 1 },
            };

            var symbols = stocks.Select(s => s.Symbol).ToList();

            var portfolio = new Portfolio
            {
                Allocations = stocks.Select(s => new StockAllocation
                {
                    Symbol = s.Symbol,
                    AllocationType = s.Type,
                    AllocationAmount = s.DesiredAmount
                }).ToList()
            };
            
            var relevantPeriods = await _alphaClient.GetPortfolioHistory(symbols);

            RunScenarioDataSet dataSet = new RunScenarioDataSet
            {
                InitialInvestment = 10000,
                CashInfluxAmount = 150,
                CashInfluxCadence = CadenceTypeEnum.Weekly,
                Portfolio = portfolio,
                History = relevantPeriods
            };

            var result = await _rebalanceLogic.RunScenario(dataSet);

            //foreach (var action in result.Actions)
            //{
            //    Console.WriteLine($"{action.ActionType} {action.Amount} shares of {action.Symbol}");
            //}
            //
            Console.WriteLine($"Portfolio grew by {result.PercentIncrease * 100} percent from {result.StartDate.ToShortDateString()} to {result.EndDate.ToShortDateString()}");
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
                new { Symbol = "SOXL", Type = AllocationTypeEnum.Percentage, DesiredAmount = 5.5m, CurrentAmount = 3 },
                new { Symbol = "CIBR", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentAmount = 13 },
                new { Symbol = "MJ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 1m, CurrentAmount = 12 },
                new { Symbol = "TAN", Type = AllocationTypeEnum.Percentage, DesiredAmount = 4m, CurrentAmount = 16 },
                new { Symbol = "ICLN", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3.5m, CurrentAmount = 40 },
                new { Symbol = "XBI", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2.5m, CurrentAmount = 4 },
                new { Symbol = "ARKG", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2.75m, CurrentAmount = 10 },
                new { Symbol = "XLF", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentAmount = 13 },
                new { Symbol = "SPY", Type = AllocationTypeEnum.Percentage, DesiredAmount = 14m, CurrentAmount = 6 },
                new { Symbol = "QQQ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 19m, CurrentAmount = 11 },
                new { Symbol = "DIA", Type = AllocationTypeEnum.Percentage, DesiredAmount = 12m, CurrentAmount = 5 },
                new { Symbol = "BND", Type = AllocationTypeEnum.Percentage, DesiredAmount = 18m, CurrentAmount = 25 },
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
                CashOnHand = 32.08m,
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