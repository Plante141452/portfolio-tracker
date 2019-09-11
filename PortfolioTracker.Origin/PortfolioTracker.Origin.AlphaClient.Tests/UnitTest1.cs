using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PortfolioTracker.Origin.AlphaClient.Tests
{
    [TestFixture]
    public class UnitTest1
    {
        private AlphaClient _alphaClient;

        [SetUp]
        public void Setup()
        {
            _alphaClient = new AlphaClient(new ApiKeyProvider());
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
                new { Symbol = "CIBR", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentAmount = 12 },
                new { Symbol = "MJ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 1m, CurrentAmount = 12 },
                new { Symbol = "TAN", Type = AllocationTypeEnum.Percentage, DesiredAmount = 4m, CurrentAmount = 16 },
                new { Symbol = "ICLN", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3.5m, CurrentAmount = 38 },
                new { Symbol = "XBI", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2.5m, CurrentAmount = 4 },
                new { Symbol = "ARKG", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2.5m, CurrentAmount = 10 },
                new { Symbol = "XLF", Type = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentAmount = 13 },
                new { Symbol = "SPY", Type = AllocationTypeEnum.Percentage, DesiredAmount = 14m, CurrentAmount = 6 },
                new { Symbol = "QQQ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 19m, CurrentAmount = 11 },
                new { Symbol = "DIA", Type = AllocationTypeEnum.Percentage, DesiredAmount = 12m, CurrentAmount = 5 },
                new { Symbol = "BND", Type = AllocationTypeEnum.Percentage, DesiredAmount = 18m, CurrentAmount = 26 },
                new { Symbol = "VYM", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2m, CurrentAmount = 2 },
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

            RebalanceDataSet dataSet = new RebalanceDataSet
            {
                CashOnHand = 86,
                ActualAllocations = actualAllocations,
                Portfolio = portfolio
            };

            var result = await _alphaClient.Rebalance(dataSet);

            foreach (var action in result.Actions)
            {
                Console.WriteLine($"{action.ActionType} {action.Amount} shares of {action.Symbol}");
            }
        }
    }
}