﻿namespace PortfolioTracker.Origin.RebalanceLogic.Models
{
    public class RebalanceAction
    {
        public string Symbol { get; set; }
        public RebalanceActionTypeEnum ActionType { get; set; }
        public int Amount { get; set; }
    }
}