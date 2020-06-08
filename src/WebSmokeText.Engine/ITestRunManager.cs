using SmokeTest.Shared;
using SmokeTest.Shared.Classes;

namespace SmokeTest.Engine
{
    public interface ITestRunManager
    {
        void ProcessTests();

        void CancelAll();

        void RunTest(in TestSchedule testSchedule);

        TestQueueItem[] QueuePositions(in long testId);

        bool CancelQueuedItem(in long testId);

        bool TestRunning(in long testId);

        TestItem[] ActiveTests(in long testId);

        TestItem[] RunningTests { get; }

        TestQueueItem[] QueuedTests { get; }
    }
}
