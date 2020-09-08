using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace PortfolioTracker.Client.Tests
{
    public class AlphaQueueWrapperTests
    {
        [Test]
        public async Task Test()
        {
            AlphaQueueWrapper wrapper = new AlphaQueueWrapper();

            await wrapper.AlphaQueue.QueueMessage(new QueueMessage { EventType = AlphaQueueWrapper.UpdateWeeklyEvent, Content = "MSFT" });
            await wrapper.AlphaQueue.QueueMessage(new QueueMessage { EventType = AlphaQueueWrapper.UpdateWeeklyEvent, Content = "SPY" });
            await wrapper.AlphaQueue.QueueMessage(new QueueMessage { EventType = AlphaQueueWrapper.UpdateWeeklyEvent, Content = "QQQ" });
            await wrapper.AlphaQueue.QueueMessage(new QueueMessage { EventType = AlphaQueueWrapper.UpdateWeeklyEvent, Content = "DIA" });
            await wrapper.AlphaQueue.QueueMessage(new QueueMessage { EventType = AlphaQueueWrapper.UpdateWeeklyEvent, Content = "QCOM" });
            await wrapper.AlphaQueue.QueueMessage(new QueueMessage { EventType = AlphaQueueWrapper.UpdateWeeklyEvent, Content = "SQ" });
            await wrapper.AlphaQueue.QueueMessage(new QueueMessage { EventType = AlphaQueueWrapper.UpdateWeeklyEvent, Content = "V" });

            wrapper.StartQueueListener();
        }
        [Test]
        public async Task Test2()
        {
            AlphaQueueWrapper wrapper = new AlphaQueueWrapper();

            await wrapper.AlphaQueue.QueueMessage(new QueueMessage { EventType = AlphaQueueWrapper.UpdateMetricsEvent, Content = "QCOM" });

            wrapper.StartQueueListener();

            Thread.Sleep(120000);
        }
    }
}
