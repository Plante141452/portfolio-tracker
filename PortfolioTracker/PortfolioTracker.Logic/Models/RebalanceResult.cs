using System.Collections.Generic;

namespace PortfolioTracker.Logic.Models
{
    public class RebalanceResult
    {
        public double RemainingCashOnHand { get; set; }
        public List<RebalanceAction> Actions { get; set; }
    }
}