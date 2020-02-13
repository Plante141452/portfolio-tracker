using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

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
    }
}
