using NUnit.Framework;
using PortfolioTracker.DataAccess;
using PortfolioTracker.Models;
using PortfolioTracker.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortfolioTracker.Logic.Tests
{
    [TestFixture]
    public class PortfolioDataAccessTests
    {
        private PortfolioDataAccess _portfolioDataAccess;

        [SetUp]
        public void Setup()
        {
            var mongoWrapper = new MongoClientWrapper(new ConnectionStringProvider());
            _portfolioDataAccess = new PortfolioDataAccess(mongoWrapper);
        }

        [Test]
        public async Task UpdatePortfolio()
        {
            var indexes = new Category //35
            {
                Name = "Indexes",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "SPY", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 14m, CurrentShares = 9 }, //14
                    new StockAllocation { Symbol = "QQQ", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 19m, CurrentShares = 16 }, //14
                    new StockAllocation { Symbol = "DIA", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 0m, CurrentShares = 6 }, //14
                    new StockAllocation { Symbol = "EEM", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentShares = 14 } //12
                }
            };

            var bonds = new Category //10
            {
                Name = "Bonds",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "TLT", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 10m, CurrentShares = 12 } //14
                }
            };


            //9.5
            var techEtFs = new Category //10
            {
                Name = "Tech",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "CIBR", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentShares = 21 }, //3
                    new StockAllocation { Symbol = "LIT", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 2m, CurrentShares = 14 }, //2.5
                    //new StockAllocation { Symbol = "SKYY", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentShares = 0 }, //2.5
                    //new StockAllocation { Symbol = "ROBO", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 2m, CurrentShares = 0 }, //2.5
                }
            };

            var cdEtFs = new Category //4
            {
                Name = "Consumer Discretionary",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "XLY", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 2m, CurrentShares = 3 }, //3
                    new StockAllocation { Symbol = "MJ", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 2m, CurrentShares = 36 } //3
                }
            };

            var energyEtFs = new Category //8
            {
                Name = "Energy",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "ICLN", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 4m, CurrentShares = 65 }, //4
                    new StockAllocation { Symbol = "TAN", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 4m, CurrentShares = 20 }, //4
                }
            };

            var bioEtFs = new Category //8
            {
                Name = "BioTech",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "XBI", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 4m, CurrentShares = 7 }, //4
                    new StockAllocation { Symbol = "ARKG", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 4m, CurrentShares = 16 } //4
                }
            };

            var financeEtFs = new Category //3
            {
                Name = "Finance",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "XLF", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentShares = 29 } //3
                }
            };

            var lowRiskStocks = new Category //6.5
            {
                Name = "Low Risk Stocks",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "MSFT", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 2m, CurrentShares = 2 },
                    new StockAllocation { Symbol = "V", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 1.7m, CurrentShares = 2 },
                    new StockAllocation { Symbol = "DIS", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 1.5m, CurrentShares = 3 },
                    new StockAllocation { Symbol = "QCOM", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 1.3m, CurrentShares = 3 }, //1
                }
            };
            //5
            var mediumRiskStocks = new Category //4.5
            {
                Name = "Medium Risk Stocks",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "SQ", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = .6m, CurrentShares = 1 }, //1
                    new StockAllocation { Symbol = "DELL", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = .8m, CurrentShares = 3 }, //1
                    new StockAllocation { Symbol = "TWTR", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = .7m, CurrentShares = 4 }, //1
                    new StockAllocation { Symbol = "PTON", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = .5m, CurrentShares = 2 }, //1
                    new StockAllocation { Symbol = "PLUG", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = .4m, CurrentShares = 12 }, //1
                    //new StockAllocation { Symbol = "SPOT", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = .6m, CurrentShares = 0 }, //1
                }
            };

            var highRisk = new Category //1
            {
                Name = "High Risk Stocks",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "CCL", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = .45m, CurrentShares = 8 }, //.25
                    new StockAllocation { Symbol = "EROS", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = .35m, CurrentShares = 30 }, //.25
                    new StockAllocation { Symbol = "TRXC", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = .15m, CurrentShares = 124 }, //.25
                    new StockAllocation { Symbol = "TRNF", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = .05m, CurrentShares = 230 }, //.25
                }
            };

            var leveraged = new Category //12
            {
                Name = "Leveraged",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "TQQQ", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 1m, CurrentShares = 1 }, //1
                    new StockAllocation { Symbol = "SOXL", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 6m, CurrentShares = 6 }, //5
                    new StockAllocation { Symbol = "FNGU", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 1m, CurrentShares = 1 }, //1
                    new StockAllocation { Symbol = "UVXY", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 4m, CurrentShares = 33 } //3
                }
            };

            Portfolio portfolio = new Portfolio
            {
                Id = "5d80d0587d2d4657d8e1fe8f",
                Name = "Default",
                CashOnHand = 29.65m,
                Categories = new List<Category>
                {
                    indexes,
                    bonds,
                    new Category
                    {
                        Name = "ETFs",
                        Categories = new List<Category>
                        {
                            techEtFs,
                            cdEtFs,
                            energyEtFs,
                            bioEtFs,
                            financeEtFs
                        }
                    },
                    lowRiskStocks,
                    mediumRiskStocks,
                    highRisk,
                    leveraged
                }
            };

            var result = await _portfolioDataAccess.SavePortfolios(new List<Portfolio> { portfolio });
            Console.WriteLine($"Portfolio Id: {result.First().Id}");
            Console.WriteLine($"Portfolio Percent: {portfolio.AllStocks.Sum(s => s.DesiredAmount)}%");
        }
    }
}