﻿using System;

namespace PortfolioTracker.Origin.Common.Models
{
    public class StockHistoryItem
    {
        public DateTimeOffset ClosingDate { get; set; }
        public long Volume { get; set; }
        public decimal AdjustedClose { get; set; }
        public decimal AdjustedPercentChanged { get; set; }
    }
}