using System;

using SmokeTest.Shared;
using SmokeTest.Shared.Classes;

namespace SmokeTest.Engine
{
    public class TestRunItem : TestItem
    {
        public TestRunItem(in ThreadWebsiteScan websiteScan, in TestSchedule testSchedule)
            : base()
        {
            WebsiteScan = websiteScan ?? throw new ArgumentNullException(nameof(websiteScan));
            UniqueId = websiteScan.UniqueId;
            Test = testSchedule ?? throw new ArgumentNullException(nameof(testSchedule));
            TestId = testSchedule.UniqueId;
            Start = DateTime.UtcNow.Ticks;
        }

        public ThreadWebsiteScan WebsiteScan { get; private set; }

        public TestSchedule Test { get; private set; }
    }
}
