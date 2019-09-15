using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Origin.Common.Models;
using PortfolioTracker.Origin.Common.Models.Enums;

namespace PortfolioTracker.Origin.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        // GET api/values/5
        [HttpGet("{user}/Portfolios")]
        public ActionResult<List<Portfolio>> Get(int id)
        {
            //new { Symbol = "DIS", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2m, CurrentAmount = 1 },
            //new { Symbol = "SQ", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2m, CurrentAmount = 3 },
            //new { Symbol = "V", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2m, CurrentAmount = 1 },
            //new { Symbol = "MSFT", Type = AllocationTypeEnum.Percentage, DesiredAmount = 2m, CurrentAmount = 1 },

            var lowRiskStocks = new Category
            {
                Name = "Low Risk Stocks",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "DIS", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 2m, CurrentShares = 1 },
                    new StockAllocation { Symbol = "SQ", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 2m, CurrentShares = 3 },
                    new StockAllocation { Symbol = "V", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 2m, CurrentShares = 1 },
                    new StockAllocation { Symbol = "MSFT", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 2m, CurrentShares = 1 }
                }
            };

            var highRisk = new Category
            {
                Name = "High Risk Stocks",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "SLRX", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 1m, CurrentShares = 11 },
                    new StockAllocation { Symbol = "TRXC", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = .15m, CurrentShares = 24 },
                    new StockAllocation { Symbol = "EROS", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = .2m, CurrentShares = 10 },
                    new StockAllocation { Symbol = "TRNX", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = .15m, CurrentShares = 8 }
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
                    new StockAllocation { Symbol = "ARKG", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 3m, CurrentShares = 10 }
                }
            };

            var financeEtFs = new Category
            {
                Name = "Finance",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "XLF", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 6m, CurrentShares = 14 }
                }
            };

            var indexes = new Category
            {
                Name = "Indexes",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "SPY", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 14m, CurrentShares = 7 },
                    new StockAllocation { Symbol = "QQQ", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 14m, CurrentShares = 8 },
                    new StockAllocation { Symbol = "DIA", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 12m, CurrentShares = 5 }
                }
            };

            var bonds = new Category
            {
                Name = "Bonds",
                Stocks = new List<StockAllocation>
                {
                    new StockAllocation { Symbol = "BND", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 15m, CurrentShares = 22 },
                    new StockAllocation { Symbol = "TLT", DesiredAmountType = AllocationTypeEnum.Percentage, DesiredAmount = 5m, CurrentShares = 4 }
                }
            };
            List<Portfolio> portfolios = new List<Portfolio>
            {
                new Portfolio
                {
                    CashOnHand = 150.15m,
                    Categories = new List<Category>
                    {
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
                        lowRiskStocks,
                        highRisk
                    }
                }
            };

            return new ActionResult<List<Portfolio>>(portfolios);
        }
    }
}