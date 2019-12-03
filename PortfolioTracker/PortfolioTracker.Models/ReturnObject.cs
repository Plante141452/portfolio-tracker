using System.Collections.Generic;

namespace PortfolioTracker.Models
{
    public class ReturnObject
    {
        public bool Success { get; set; }
        public List<ReturnMessage> Messages { get; set; }
    }

    public class ReturnObject<T> : ReturnObject
    {
        public T Data { get; set; }
    }
}
