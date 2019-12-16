using System.Threading.Tasks;

namespace PortfolioTracker.Logic.Interfaces
{
    public interface IAzureQueueClient
    {
        Task SendMessageAsync(string queue, string symbol, int msDelay);
    }
}