namespace PortfolioTracker.DataAccess.DataTypes
{
    public class Portfolio : Category
    {
        public virtual string Id { get; set; }
        public decimal CashOnHand { get; set; }
    }
}