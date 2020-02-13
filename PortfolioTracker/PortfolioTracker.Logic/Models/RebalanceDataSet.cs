using System.Collections.Generic;
using PortfolioTracker.Models;

namespace PortfolioTracker.Logic.Models
{
    public class RebalanceDataSet
    {
        public Portfolio Portfolio { get; set; }
        public List<Quote> Quotes { get; set; }
    }
}