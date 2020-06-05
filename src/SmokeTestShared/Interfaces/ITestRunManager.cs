using SmokeTest.Shared.Classes;

namespace SmokeTest.Shared
{
    public interface ITestRunManager
    {
        void ProcessTests();

        void CancelAll();

        void RunTest(in TestSchedule testSchedule);

        int[] QueuePositions(in long testId);

        bool TestRunning(in long testId);

        TestItem[] ActiveTests(in long testId);

        TestItem[] RunningTests { get; }

        TestItem[] QueuedTests { get; }
    }
}
