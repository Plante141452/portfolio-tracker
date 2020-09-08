namespace PortfolioTracker.Models
{
    public class Portfolio : Category
    {
        public virtual string Id { get; set; }
        public double CashOnHand { get; set; }
    }
}