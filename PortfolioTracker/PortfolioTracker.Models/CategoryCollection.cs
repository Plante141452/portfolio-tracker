using System.Collections.Generic;
using System.Linq;

namespace PortfolioTracker.Models
{
    public class CategoryCollection
    {
        public List<Category> Categories { get; set; }
        public List<StockAllocation> Stocks { get; set; }

        public List<StockAllocation> AllStocks
        {
            get
            {
                var stocks = Stocks ?? new List<StockAllocation>();
                if (Categories != null)
                    stocks.AddRange(Categories.SelectMany(c => c.AllStocks));
                return stocks;
            }
        }
    }
}