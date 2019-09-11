using System.Collections.Generic;

namespace PortfolioTracker.Origin.RebalanceLogic.Models
{
    public class RebalanceResult
    {
        public decimal RemainingCashOnHand { get; set; }
        public List<RebalanceAction> Actions { get; set; }
    }
}