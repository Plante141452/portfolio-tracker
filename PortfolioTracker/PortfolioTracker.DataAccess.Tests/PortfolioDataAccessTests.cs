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
                Name = "Low Risk",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "SPY", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 8, PurchaseRange = .01, CurrentShares = 3 }, //14
                    new StockAllocation { Symbol = "QQQ", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 12, PurchaseRange = .01, CurrentShares = 8 }, //14
                    new StockAllocation { Symbol = "EEM", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3, PurchaseRange = .01, CurrentShares = 9 } //12
                }
            };

            var bonds = new Category //10
            {
                Name = "Safety",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "GLD", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 4.5, PurchaseRange = .01, CurrentShares = 0 }, //14
                    new StockAllocation { Symbol = "TLT", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 4.5, PurchaseRange = .01, CurrentShares = 12 }, //14
                    new StockAllocation { Symbol = "UVXY", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3, PurchaseRange = .03, CurrentShares = 33 } //3
                }
            };

            var semiEtFs = new Category
            {
                Name = "Chips",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "SMH", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 6, PurchaseRange = .02, CurrentShares = 5 }, //3
                    new StockAllocation { Symbol = "QCOM", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 2, PurchaseRange = .02, CurrentShares = 0 }, //1
                }
            };

            var cloudEtFs = new Category
            {
                Name = "Cloud",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "ARKW", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3.5, PurchaseRange = .02, CurrentShares = 8 }, //3
                    new StockAllocation { Symbol = "WCLD", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 2.5, PurchaseRange = .02, CurrentShares = 13 }, //2.5
                    new StockAllocation { Symbol = "MSFT", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 2, PurchaseRange = .02, CurrentShares = 2 },
                }
            };

            var securityEtFs = new Category
            {
                Name = "Security",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "CIBR", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 2.75, PurchaseRange = .02, CurrentShares = 15 }, //3
                    new StockAllocation { Symbol = "CSCO", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 1, PurchaseRange = .02, CurrentShares = 0 }, //2.5
                }
            };

            var batteryEtFs = new Category
            {
                Name = "Batteries",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "LIT", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 4, PurchaseRange = .02, CurrentShares = 6 }, //2.5
                    new StockAllocation { Symbol = "ENS", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 1, PurchaseRange = .02, CurrentShares = 1 }, //2.5
                }
            };

            var techEtFs = new Category //10
            {
                Name = "Tech",
                Categories = new List<Category> { semiEtFs, cloudEtFs, securityEtFs, batteryEtFs }
            };

            var innovationEtFs = new Category
            {
                Name = "Innovation",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "ARKK", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3, PurchaseRange = .02, CurrentShares = 9 },
                }
            };

            var cdEtFs = new Category //4
            {
                Name = "Consumer Discretionary",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "VCR", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3.5, PurchaseRange = .02, CurrentShares = 2 }, //3
                    new StockAllocation { Symbol = "YOLO", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 1.5, PurchaseRange = .02, CurrentShares = 22 }, //3
                    new StockAllocation { Symbol = "LOW", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 1, PurchaseRange = .02, CurrentShares = 1 }, //3
                }
            };

            var energyEtFs = new Category //8
            {
                Name = "Clean Energy",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "QCLN", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3.5, PurchaseRange = .02, CurrentShares = 15 }, //4
                    new StockAllocation { Symbol = "TAN", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 4, PurchaseRange = .02, CurrentShares = 16 }, //4
                    new StockAllocation { Symbol = "HASI", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 1, PurchaseRange = .02, CurrentShares = 6 }, //4
                }
            };

            var bioEtFs = new Category //8
            {
                Name = "BioTech",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "ARKG", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 2.5, PurchaseRange = .02, CurrentShares = 5 }, //4
                    new StockAllocation { Symbol = "XBI", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 1.5, PurchaseRange = .02, CurrentShares = 2 }, //4
                    new StockAllocation { Symbol = "CDNA", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 1, PurchaseRange = .02, CurrentShares = 8 } //4
                }
            };

            var financeEtFs = new Category //3
            {
                Name = "Finance",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "ARKF", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3, PurchaseRange = .02, CurrentShares = 19 }, //3
                    new StockAllocation { Symbol = "XLF", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 2, PurchaseRange = .02, CurrentShares = 10 }, //3
                    new StockAllocation { Symbol = "SQ", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 1, PurchaseRange = .02, CurrentShares = 2 }
                }
            };

            var leveraged = new Category //12
            {
                Name = "Leveraged",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "TQQQ", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 4, PurchaseRange = .03, CurrentShares = 7 }, //1
                    new StockAllocation { Symbol = "SOXL", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3, PurchaseRange = .03, CurrentShares = 3 }, //5
                    new StockAllocation { Symbol = "FNGU", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3, PurchaseRange = .03, CurrentShares = 4 }, //1
                }
            };

            var funStocks = new Category //6.5
            {
                Name = "Fun Stuff",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "TRMB", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 1.5, PurchaseRange = .02, CurrentShares = 9 },
                    new StockAllocation { Symbol = "UBER", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = .5, PurchaseRange = .02, CurrentShares = 3 },
                    new StockAllocation { Symbol = "MDB", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = .5, PurchaseRange = .02, CurrentShares = 1 },
                    new StockAllocation { Symbol = "CCL", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = .5, PurchaseRange = .02, CurrentShares = 5 },
                    new StockAllocation { Symbol = "TRXC", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 0, PurchaseRange = .02, CurrentShares = 98 },
                }
            };

            Portfolio portfolio = new Portfolio
            {
                Id = "5d80d0587d2d4657d8e1fe8f",
                Name = "Default",
                CashOnHand = 10193.38,
                Categories = new List<Category>
                {
                    indexes,
                    bonds,
                    new Category
                    {
                        Name = "ETFs",
                        Categories = new List<Category>
                        {
                            innovationEtFs,
                            techEtFs,
                            cdEtFs,
                            energyEtFs,
                            bioEtFs,
                            financeEtFs
                        }
                    },
                    leveraged,
                    funStocks
                }
            };

            var result = await _portfolioDataAccess.SavePortfolios(new List<Portfolio> { portfolio });
            Console.WriteLine($"Portfolio Id: {result.First().Id}");
            Console.WriteLine($"Portfolio Percent: {portfolio.AllStocks.Sum(s => s.DesiredAmount)}%");
        }
    }
}