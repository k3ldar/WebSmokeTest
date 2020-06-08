using System;

using SmokeTest.Shared;
using SmokeTest.Shared.Classes;
using SmokeTest.Shared.Engine;

namespace SmokeTest.Engine
{
    public class TestQueueItem : TestItem
    {
        public TestQueueItem(in long queueId, in SmokeTestProperties smokeTestProperties, in TestSchedule testSchedule)
            : base()
        {
            SmokeTestProperties = smokeTestProperties ?? throw new ArgumentNullException(nameof(smokeTestProperties));
            Test = testSchedule ?? throw new ArgumentNullException(nameof(testSchedule));
            TestId = testSchedule.UniqueId;
            Start = DateTime.UtcNow.Ticks;
            QueueId = queueId;
        }

        public SmokeTestProperties SmokeTestProperties { get; private set; }

        public TestSchedule Test { get; private set; }

        public long QueueId { get; private set; }
    }
}
